namespace SuperheroSurvivors.Core;

/// Один модификатор стата. Итог: (база + Σ аддитивных) × Π мультипликативных.
/// SourceId позволяет снимать/заменять модификаторы по источнику (баффы, предметы).
public class StatModifier
{
    public string SourceId { get; }
    public float Additive { get; }
    public float Multiplicative { get; }

    public StatModifier(string sourceId, float additive = 0f, float multiplicative = 1f)
    {
        SourceId = sourceId;
        Additive = additive;
        Multiplicative = multiplicative;
    }
}
