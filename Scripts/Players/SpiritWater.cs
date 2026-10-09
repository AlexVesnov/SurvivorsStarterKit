using Godot;
using System.Collections.Generic;
using SuperheroSurvivors.Core;

/// Авто-оружие: ядовитая зона, появляющаяся в случайной точке вокруг героя.
/// Наносит урон всем врагам внутри раз в тик, исчезает через Duration.
public partial class SpiritWater : AutoWeapon
{
    [Export]
    public float Damages = 1f;

    [Export]
    public float Duration = 4f;

    /// Интервал между появлениями зон, в секундах.
    [Export]
    public float Cooldown = 5f;

    [Export]
    public float ProjectileRange = 2f;

    [Export]
    public PackedScene ProjectilePrefab;

    public override string WeaponId => "spirit_water";

    private readonly List<Enemy> _enemiesInZone = new();
    private Timer _projectileCooldown;
    private Timer _damageCooldown;

    public override void _Ready()
    {
        _projectileCooldown = GetNode<Timer>("ProjectileCooldown");
        _projectileCooldown.WaitTime = Cooldown;
        _projectileCooldown.Timeout += OnAttackReady;
        _projectileCooldown.Start();

        _damageCooldown = GetNode<Timer>("DamageCooldown");
        _damageCooldown.Timeout += OnDamageTick;
        _damageCooldown.Start();
    }

    public void AddDamageBonus(int bonus) => Damages += bonus;

    public void AddCooldownBonus(float bonus)
    {
        Cooldown = Mathf.Max(1f, Cooldown - bonus);
        _projectileCooldown.WaitTime = Cooldown;
    }

    private void OnAttackReady()
    {
        _enemiesInZone.Clear();
        var zone = ProjectilePrefab.Instantiate<Area3D>();
        zone.BodyEntered += OnBodyEntered;
        zone.BodyExited += OnBodyExited;
        GetTree().CurrentScene.AddChild(zone);
        zone.GlobalPosition = GameManager.Instance.GetRandomPosAroundPlayer(ProjectileRange)
                              + new Vector3(0, 0.1f, 0);
        DespawnAfter(zone, Duration);
    }

    private void OnDamageTick()
    {
        for (int i = _enemiesInZone.Count - 1; i >= 0; i--)
        {
            if (!GodotObject.IsInstanceValid(_enemiesInZone[i]))
            {
                _enemiesInZone.RemoveAt(i);
                continue;
            }
            DealDamage(_enemiesInZone[i], Damages, DamageType.Fire);
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Enemy enemy && !_enemiesInZone.Contains(enemy))
            _enemiesInZone.Add(enemy);
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is Enemy enemy)
            _enemiesInZone.Remove(enemy);
    }

    private async void DespawnAfter(Area3D zone, float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(zone)) zone.QueueFree();
    }
}
