namespace SuperheroSurvivors.Core;

/// Урон всегда передаётся как DamageInfo, никогда голым числом.
/// База для статусов, комбо-детектора и аналитики: у каждого попадания
/// есть источник, тип и флаг «способность или авто-оружие».
/// Класс намеренно без зависимостей от Godot — тестируется юнит-тестами.
public class DamageInfo
{
    public float Amount { get; }
    public string SourceId { get; }
    public DamageType Type { get; }
    public bool IsAbility { get; }
    public bool IsCrit { get; }

    private DamageInfo(float amount, string sourceId, DamageType type, bool isAbility, bool isCrit)
    {
        Amount = amount;
        SourceId = sourceId;
        Type = type;
        IsAbility = isAbility;
        IsCrit = isCrit;
    }

    public static DamageInfo Auto(string sourceId, float amount, DamageType type)
        => new(amount, sourceId, type, isAbility: false, isCrit: false);

    public static DamageInfo Ability(string sourceId, float amount, DamageType type)
        => new(amount, sourceId, type, isAbility: true, isCrit: false);

    public DamageInfo WithAmount(float newAmount) => new(newAmount, SourceId, Type, IsAbility, IsCrit);

    public DamageInfo AsCrit() => new(Amount, SourceId, Type, IsAbility, isCrit: true);
}
