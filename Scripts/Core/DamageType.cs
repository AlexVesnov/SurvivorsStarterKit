namespace SuperheroSurvivors.Core;

/// Тип урона. Теги синергий из дизайн-документа: огонь / тех / кинетика.
/// Fire пока покрывает и яд/химию (SpiritWater) — уточнить теги в M2.
public enum DamageType
{
    Kinetic,
    Fire,
    Tech,
}
