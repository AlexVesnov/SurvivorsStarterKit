using Godot;
using SuperheroSurvivors.Enemies;
using System.Collections.Generic;

/// Спавн и поведение орды. Кривые по игровому времени — никаких голосований.
/// Разделение врагов: централизованный проход парами каждый 2-й физический тик
/// (O(n²) при потолке ~200 врагов — ~20 тыс. проверок, дёшево в C#).
internal class EnemyManager
{
    public const int EnemySpawnRange = 30;
    public const int MaxEnemies = 200;
    private const float SeparationRadius = 1.4f;
    private const float SeparationForce = 2.5f;

    private readonly GameManager _gameManager;
    private readonly Dictionary<EnemyClass, PackedScene> _enemyPrefabs;
    private readonly List<Enemy> _enemies = new();
    public IReadOnlyList<Enemy> Enemies => _enemies;

    private int _separationTick;

    public EnemyManager(GameManager gameManager)
    {
        _gameManager = gameManager;
        _enemyPrefabs = new()
        {
            { EnemyClass.Minion, GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_minion.tscn") },
            { EnemyClass.Warrior, GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_warrior.tscn") },
            { EnemyClass.Archer, GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_archer.tscn") },
            { EnemyClass.Mage, GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_mage.tscn") },
            { EnemyClass.Boss, GD.Load<PackedScene>("res://Prefabs/Enemies/enemy_boss.tscn") },
        };
    }

    /// Интервал спавна: от 1,0 с до 0,22 с к 10-й минуте. Рост плавный, по времени.
    public float SpawnInterval(double gameTime)
        => Mathf.Max(0.22f, 1.0f / (1.0f + (float)gameTime / 45f));

    /// Сколько врагов за один тик спавна: 1 → +1 каждую минуту (потолок 4).
    public int SpawnBatch(double gameTime)
        => Mathf.Clamp(1 + (int)(gameTime / 60.0), 1, 4);

    /// Состав орды по времени: миньоны → воины (1 мин) → лучники (2,5 мин) → маги (4 мин).
    private EnemyClass PickClass(double gameTime)
    {
        float roll = GD.Randf();
        if (gameTime > 240 && roll < 0.15f) return EnemyClass.Mage;
        if (gameTime > 150 && roll < 0.25f) return EnemyClass.Archer;
        if (gameTime > 60 && roll < 0.35f) return EnemyClass.Warrior;
        return EnemyClass.Minion;
    }

    /// Рост здоровья: +100% к 3-й минуте, +300% к 10-й (мягкая экспонента).
    private int HpMultiplier(double gameTime) => 1 + (int)(gameTime / 90.0);

    public Enemy SpawnEnemy(double gameTime) => SpawnEnemy(PickClass(gameTime), gameTime);

    public Enemy SpawnBoss(double gameTime) => SpawnEnemy(EnemyClass.Boss, gameTime);

    private Enemy SpawnEnemy(EnemyClass enemyClass, double gameTime)
    {
        var enemy = _enemyPrefabs[enemyClass].Instantiate<Enemy>();
        enemy.Name = enemyClass.ToString();
        enemy.Lifepoints = enemy.Lifepoints * HpMultiplier(gameTime);
        enemy.Position = _gameManager.GetRandomPosAroundPlayer(EnemySpawnRange);
        enemy.TreeExiting += () => _enemies.Remove(enemy);
        _gameManager.GetNode("/root/MainScene").AddChild(enemy);
        _enemies.Add(enemy);
        return enemy;
    }

    public void TickSeparation()
    {
        if (++_separationTick % 2 != 0) return; // 30 Гц вместо 60

        float radiusSq = SeparationRadius * SeparationRadius;
        for (int i = 0; i < _enemies.Count; i++)
        {
            var a = _enemies[i];
            if (!GodotObject.IsInstanceValid(a)) continue;
            for (int j = i + 1; j < _enemies.Count; j++)
            {
                var b = _enemies[j];
                if (!GodotObject.IsInstanceValid(b)) continue;

                Vector3 diff = a.GlobalPosition - b.GlobalPosition;
                float distSq = diff.LengthSquared();
                if (distSq >= radiusSq || distSq < Mathf.Epsilon) continue;

                Vector3 push = diff.Normalized() * SeparationForce * (1f - distSq / radiusSq);
                a.SeparationPush += push;
                b.SeparationPush -= push;
            }
        }
    }

    /// Ближайший враг — линейный проход O(n), без сортировки (ошибка кита исправлена).
    public Enemy Nearest(Vector3 from)
    {
        Enemy best = null;
        float bestSq = float.MaxValue;
        foreach (var enemy in _enemies)
        {
            if (!GodotObject.IsInstanceValid(enemy)) continue;
            float distSq = (enemy.GlobalPosition - from).LengthSquared();
            if (distSq >= bestSq) continue;
            bestSq = distSq;
            best = enemy;
        }
        return best;
    }
}
