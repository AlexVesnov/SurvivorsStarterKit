using Godot;
using System;
using SuperheroSurvivors.Core;

public partial class Player : CharacterBody3D
{
    [Export]
    public float Speed { get; private set; } = 5.0f;

    public StatBlock Stats { get; } = new();

    private float _lifepoints;

    private GameManager _gameManager;
    private Node3D _visual;
    private AnimationTree _animationTree;
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public float CurrentHealth => _lifepoints;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        _gameManager.Player = this;

        Stats.SetBase(StatType.MaxHealth, 200f);
        Stats.SetBase(StatType.Armor, 0f);
        Stats.SetBase(StatType.MoveSpeed, Speed);
        Stats.SetBase(StatType.Damage, 1f);
        Stats.SetBase(StatType.AttackSpeed, 1f);
        Stats.SetBase(StatType.CritChance, 0.05f);
        Stats.SetBase(StatType.CritMultiplier, 2f);
        Stats.SetBase(StatType.PickupRadius, 2.5f);
        Stats.SetBase(StatType.AreaScale, 1f);
        _lifepoints = Stats.Final(StatType.MaxHealth);

        _visual = GetNode<Node3D>("Visual");
        _animationTree = GetNode<AnimationTree>("AnimationTree");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        float speed = Stats.Final(StatType.MoveSpeed);
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * speed;
            velocity.Z = direction.Z * speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
        }

        Velocity = velocity;
        MoveAndSlide();

        velocity.Y = 0;
        _animationTree.Set("parameters/walking/blend_amount", velocity.LimitLength(1).Length());
        if (Mathf.IsZeroApprox(velocity.Length())) return;

        _visual.LookAt(Position + 10 * velocity, Vector3.Up, true);
    }

    public void TakeDamages(DamageInfo info)
    {
        float damage = Math.Max(1f, info.Amount - Stats.Final(StatType.Armor));
        _lifepoints = Math.Clamp(_lifepoints - damage, 0, Stats.Final(StatType.MaxHealth));
        GameEvents.RaisePlayerDamaged(damage);

        if (_lifepoints <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        _lifepoints = Math.Clamp(_lifepoints + amount, 0, Stats.Final(StatType.MaxHealth));
        GameEvents.RaisePlayerHealed(amount);
    }

    private async void Die()
    {
        GetTree().Paused = true;
        GD.Print("Герой пал. Перезапуск забега...");
        await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }
}
