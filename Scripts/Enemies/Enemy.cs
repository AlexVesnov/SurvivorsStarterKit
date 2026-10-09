using Godot;
using SuperheroSurvivors.Core;

public partial class Enemy : AnimatableBody3D
{
    [Export]
    public int Lifepoints { get; set; } = 10;

    [Export]
    public uint Damages { get; set; } = 5;

    [Export]
    public float MovementSpeed = 4f;

    [Export]
    public int Experience { get; private set; } = 1;

    /// Внешний толчок от разделения орды. Задаёт EnemyManager, обнуляется каждый тик.
    public Vector3 SeparationPush;

    private GameManager _gameManager;
    private GpuParticles3D _damageParticles;
    private Timer _attackCooldown;

    public bool IsPlayerInAttackRange => (_gameManager.Player.GlobalPosition - GlobalPosition).Length() <= 2f;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _damageParticles = GetNode<GpuParticles3D>("DamageParticles");
        _attackCooldown = GetNode<Timer>("AttackCooldown");
        AddToGroup("enemies");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 direction = (_gameManager.Player.GlobalPosition - GlobalPosition).LimitLength();
        Vector3 velocity = MovementSpeed * direction + SeparationPush;
        GlobalPosition += (float)delta * velocity;
        SeparationPush = Vector3.Zero;

        if (_attackCooldown.TimeLeft <= 0 && IsPlayerInAttackRange)
        {
            _attackCooldown.Start();
            _gameManager.Player.TakeDamages(DamageInfo.Auto("enemy", Damages, DamageType.Kinetic));
        }

        if (direction != Vector3.Zero)
            LookAt(_gameManager.Player.GlobalPosition, Vector3.Up, true);
    }

    public void TakeDamages(DamageInfo info)
    {
        _damageParticles.Restart();

        Lifepoints -= (int)info.Amount;
        if (Lifepoints > 0)
        {
            GameEvents.RaiseEnemyDamaged(this, info);
            return;
        }

        GameEvents.RaiseEnemyDamaged(this, info);
        GameEvents.RaiseEnemyDied(this);
        _gameManager.OnEnemyKilled(this);
        QueueFree();
    }
}
