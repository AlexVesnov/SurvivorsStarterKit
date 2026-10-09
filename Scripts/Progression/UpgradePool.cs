using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using SuperheroSurvivors.Core;

/// Витрина левел-апа: 4 случайных различных варианта из пула.
/// Пул 0.5 — заглушка на коде; в M1 переводим опции на данные (Resource-файлы).
/// Важно: каждая опция должна иметь ПОТРЕБИТЕЛЯ — не предлагаем то, что ничего не делает.
public static class UpgradePool
{
    public static List<LevelUpOption> Roll(Player player, int count)
    {
        var pool = BuildPool(player);
        var picked = new List<LevelUpOption>();
        while (picked.Count < count && pool.Count > 0)
        {
            int i = GD.RandRange(0, pool.Count - 1);
            picked.Add(pool[i]);
            pool.RemoveAt(i);
        }
        return picked;
    }

    private static List<LevelUpOption> BuildPool(Player player)
    {
        var options = new List<LevelUpOption>
        {
            // --- Статовые опции (потребители: Player, AutoWeapon, XpGem) ---
            StatOption(player, "Здоровье +25", "Максимальное здоровье +25 и мгновенное лечение на 25.",
                StatType.MaxHealth, additive: 25f, healAfter: 25f),
            StatOption(player, "Броня +2", "Герой получает на 2 меньше урона от каждого удара.",
                StatType.Armor, additive: 2f),
            StatOption(player, "Скорость +8%", "Герой двигается на 8% быстрее.",
                StatType.MoveSpeed, multiplicative: 1.08f),
            StatOption(player, "Урон +10%", "Всё оружие наносит на 10% больше урона.",
                StatType.Damage, multiplicative: 1.10f),
            StatOption(player, "Крит +5%", "Шанс критического удара +5%. Крит наносит двойной урон.",
                StatType.CritChance, additive: 0.05f),
            StatOption(player, "Радиус подбора +25%", "Гемы опыта притягиваются с большего расстояния.",
                StatType.PickupRadius, multiplicative: 1.25f),
        };

        // --- Апгрейды оружий (потребители: конкретные классы оружий) ---
        var shooting = player.GetChildren().OfType<Shooting>().FirstOrDefault();
        if (shooting != null)
            options.Add(new LevelUpOption("Выстрел — урон +2",
                "Пули выстрела наносят на 2 больше урона.",
                _ => shooting.AddDamageBonus(2)));

        var spheres = player.GetChildren().OfType<FloatingSphereAttack>().FirstOrDefault();
        if (spheres != null)
            options.Add(new LevelUpOption("Сферы — ещё одна",
                "Добавляет одну вращающуюся сферу вокруг героя.",
                _ => spheres.AddSphere()));

        var lifesteal = player.GetChildren().OfType<LifestealAttack>().FirstOrDefault();
        if (lifesteal != null)
            options.Add(new LevelUpOption("Вампиризм — урон +1",
                "Зона вампиризма наносит на 1 больше урона (и лечит на столько же).",
                _ => lifesteal.AddDamageBonus(1)));

        var poison = player.GetChildren().OfType<SpiritWater>().FirstOrDefault();
        if (poison != null)
        {
            options.Add(new LevelUpOption("Яд — урон +1",
                "Ядовитая зона наносит на 1 больше урона за тик.",
                _ => poison.AddDamageBonus(1)));
            options.Add(new LevelUpOption("Яд — перезарядка −",
                "Ядовитая зона появляется чаще.",
                _ => poison.AddCooldownBonus(0.3f)));
        }

        return options;
    }

    private static LevelUpOption StatOption(Player player, string title, string description,
        StatType stat, float additive = 0f, float multiplicative = 1f, float healAfter = 0f)
        => new(title, description, _ =>
        {
            player.Stats.AddModifier(stat, new StatModifier(Guid.NewGuid().ToString("N")[..8], additive, multiplicative));
            if (healAfter > 0f) player.Heal(healAfter);
            GameManager.Instance.UpdateLifeBar();
        });
}
