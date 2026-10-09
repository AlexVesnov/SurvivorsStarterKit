using System;
using Godot;
using SuperheroSurvivors.Core;

/// Центральная шина событий (C# events, не Godot-сигналы).
/// Сюда подписываются UI, комбо-детектор (M2), аналитика.
/// Все события поднимаются с физического тика — время игры, не рендера.
public static class GameEvents
{
    // Бой
    public static event Action<Enemy, DamageInfo> EnemyDamaged;
    public static event Action<Enemy> EnemyDied;
    public static event Action<float> PlayerDamaged;   // фактический урон после брони
    public static event Action<float> PlayerHealed;

    // Прогрессия
    public static event Action<float, float> XpChanged;     // (текущий, нужно для уровня)
    public static event Action<int> LevelChanged;           // новый уровень
    public static event Action<int> GoldChanged;

    public static void RaiseEnemyDamaged(Enemy enemy, DamageInfo info) => EnemyDamaged?.Invoke(enemy, info);
    public static void RaiseEnemyDied(Enemy enemy) => EnemyDied?.Invoke(enemy);
    public static void RaisePlayerDamaged(float amount) => PlayerDamaged?.Invoke(amount);
    public static void RaisePlayerHealed(float amount) => PlayerHealed?.Invoke(amount);
    public static void RaiseXpChanged(float current, float needed) => XpChanged?.Invoke(current, needed);
    public static void RaiseLevelChanged(int level) => LevelChanged?.Invoke(level);
    public static void RaiseGoldChanged(int gold) => GoldChanged?.Invoke(gold);
}
