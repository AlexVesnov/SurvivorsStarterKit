using Godot;
using SuperheroSurvivors.Core;

/// Авто-оружие: зона вокруг героя, раз в кулдаун бьёт всех врагов внутри
/// и лечит героя на сумму нанесённого урона.
public partial class LifestealAttack : AutoWeapon
{
    [Export]
    public float Damages = 1f;

    /// Кулдаун в СЕКУНДАХ между ударами (в ките семантика была перевернута — исправлено).
    [Export]
    public float Cooldown = 2f;

    public override string WeaponId => "lifesteal";

    private Area3D _area;
    private Timer _timer;

    public override void _Ready()
    {
        _area = GetNode<Area3D>("Area3D");
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = Cooldown;
        _timer.Timeout += OnTick;
        _timer.Start();
    }

    public void AddDamageBonus(int bonus) => Damages += bonus;

    private void OnTick()
    {
        if (!GodotObject.IsInstanceValid(Owner)) return;
        if (!_area.HasOverlappingBodies()) return;

        int hit = 0;
        foreach (var body in _area.GetOverlappingBodies())
        {
            if (body is not Enemy enemy) continue;
            DealDamage(enemy, Damages, DamageType.Fire);
            hit++;
        }
        if (hit > 0) Owner.Heal(Damages * hit);
    }
}
