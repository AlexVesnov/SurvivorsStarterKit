using Godot;
using SuperheroSurvivors.Core;

/// Авто-оружие: пуля в ближайшего врага. Пули существуют максимум 3 секунды
/// (в ките жили вечно при промахе — исправлено).
public partial class Shooting : AutoWeapon
{
    [Export]
    public float Damages = 5f;

    [Export]
    public float AttackSpeed = 1f;

    [Export]
    public float BulletSpeed = 8f;

    public override string WeaponId => "shooting";

    private PackedScene _bulletPrefab;
    private Timer _timer;

    public override void _Ready()
    {
        _bulletPrefab = GD.Load<PackedScene>("res://Prefabs/Powerups/bullet.tscn");
        _timer = GetNode<Timer>("Timer");
        _timer.WaitTime = 1f / AttackSpeed;
        _timer.Timeout += Shoot;
        _timer.Start();
    }

    public void AddDamageBonus(int bonus) => Damages += bonus;

    private void Shoot()
    {
        var target = FindTarget();
        if (target == null) return;

        var bullet = _bulletPrefab.Instantiate<RigidBody3D>();
        bullet.LinearVelocity = BulletSpeed * (target.GlobalPosition - GlobalPosition).Normalized();
        bullet.BodyEntered += body =>
        {
            if (body is Enemy enemy) DealDamage(enemy, Damages, DamageType.Kinetic);
            bullet.QueueFree();
        };
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);

        DespawnAfter(bullet, 3f);
    }

    private async void DespawnAfter(RigidBody3D bullet, float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(bullet)) bullet.QueueFree();
    }
}
