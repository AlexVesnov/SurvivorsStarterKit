using System;
using SuperheroSurvivors.Core;

namespace SuperheroSurvivors.Tests;

/// Мини-раннер: Check бросает при несовпадении, в конце — итог и код выхода.
/// Намеренно без xUnit/NUnit: проект должен собираться и запускаться офлайн,
/// без обращений к NuGet. Команда: dotnet run --project Tests
internal static class Program
{
    private static int _passed;
    private static int _failed;

    private static bool Eq(float a, float b) => Math.Abs(a - b) < 1e-4f;

    private static void Check(string name, bool condition)
    {
        if (condition) { _passed++; Console.WriteLine($"  ok   {name}"); }
        else { _failed++; Console.WriteLine($"  FAIL {name}"); }
    }

    private static void Main()
    {
        Console.WriteLine("StatBlock:");
        TestStatBlock();

        Console.WriteLine("DamageInfo:");
        TestDamageInfo();

        Console.WriteLine();
        Console.WriteLine($"Итого: {_passed} ок, {_failed} не ок");
        Environment.ExitCode = _failed == 0 ? 0 : 1;
    }

    private static void TestStatBlock()
    {
        // Формула: итог = (база + Σ аддитивных) × Π мультипликативных

        var s = new StatBlock();
        s.SetBase(StatType.Damage, 100f);
        Check("итог без модификаторов = база", s.Final(StatType.Damage) == 100f);

        s.AddModifier(StatType.Damage, new StatModifier("sword", additive: 10f));
        s.AddModifier(StatType.Damage, new StatModifier("buff", additive: 5f));
        Check("аддитивные суммируются: 100+10+5=115", s.Final(StatType.Damage) == 115f);

        s.AddModifier(StatType.Damage, new StatModifier("rage", multiplicative: 1.2f));
        s.AddModifier(StatType.Damage, new StatModifier("focus", multiplicative: 1.5f));
        Check("комбинация: (100+10+5)×1.2×1.5=207", Eq(s.Final(StatType.Damage), 115f * 1.2f * 1.5f));

        // Только мультипликаторы
        var m = new StatBlock();
        m.SetBase(StatType.AttackSpeed, 2f);
        m.AddModifier(StatType.AttackSpeed, new StatModifier("a", multiplicative: 1.25f));
        m.AddModifier(StatType.AttackSpeed, new StatModifier("b", multiplicative: 2f));
        Check("мультипликативные перемножаются: 2×1.25×2=5", m.Final(StatType.AttackSpeed) == 5f);

        // Снятие по источнику
        s.RemoveModifiersFrom("rage");
        Check("RemoveModifiersFrom снимает только свой источник", s.Final(StatType.Damage) == 115f * 1.5f);

        s.RemoveModifiersFrom("buff");
        Check("снятие аддитива из другого источника", s.Final(StatType.Damage) == 110f * 1.5f);

        // Модификаторы одного стата не трогают другой
        Check("модификаторы Damage не влияют на MaxHealth", s.Final(StatType.MaxHealth) == 0f);

        // Порядок не важен: аддитив всегда до мультипликатив
        var p = new StatBlock();
        p.SetBase(StatType.MoveSpeed, 10f);
        p.AddModifier(StatType.MoveSpeed, new StatModifier("boots", multiplicative: 1.5f));
        p.AddModifier(StatType.MoveSpeed, new StatModifier("haste", additive: 10f));
        Check("порядок добавления не влияет: (10+10)×1.5=30", p.Final(StatType.MoveSpeed) == 30f);
    }

    private static void TestDamageInfo()
    {
        var auto = DamageInfo.Auto("blaster", 12f, DamageType.Kinetic);
        Check("Auto: сумма и тип", auto.Amount == 12f && auto.Type == DamageType.Kinetic);
        Check("Auto: не способность, не крит", !auto.IsAbility && !auto.IsCrit);
        Check("Auto: источник сохранён", auto.SourceId == "blaster");

        var ability = DamageInfo.Ability("nova", 40f, DamageType.Fire);
        Check("Ability: флаг способности", ability.IsAbility && !ability.IsCrit);

        var crit = auto.AsCrit();
        Check("AsCrit: сумма не меняется, флаг крита", crit.Amount == 12f && crit.IsCrit);
        Check("AsCrit: исходный объект не мутирован", !auto.IsCrit);

        var scaled = ability.WithAmount(100f);
        Check("WithAmount: меняет сумму", scaled.Amount == 100f);
        Check("WithAmount: сохраняет источник/тип/флаги",
            scaled.SourceId == "nova" && scaled.Type == DamageType.Fire && scaled.IsAbility);
    }
}
