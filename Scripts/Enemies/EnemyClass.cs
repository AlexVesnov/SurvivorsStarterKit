namespace SuperheroSurvivors.Enemies
{
    /// <summary>
    /// Градация врагов по классам. Значения >= 100 — боссы (для быстрой проверки).
    /// </summary>
    public enum EnemyClass
    {
        Minion = 0,
        Warrior = 1,
        Archer = 2,
        Mage = 3,
        Boss = 100
    }
}
