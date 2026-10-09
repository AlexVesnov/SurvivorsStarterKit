using Godot;
using SuperheroSurvivors.Core;

/// Гем опыта, падающий с убитых врагов. Магнитится к герою в радиусе подбора.
/// Подбор и магнит — по расстоянию, без физических запросов (дешево при сотнях гемов).
public partial class XpGem : Area3D
{
    public float Value = 1f;

    private const float CollectDistance = 0.7f;
    private const float Lifetime = 40f;

    private double _lifetime;
    private MeshInstance3D _mesh;

    public override void _Ready()
    {
        _mesh = GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        _lifetime += delta;
        if (_lifetime > Lifetime)
        {
            QueueFree();
            return;
        }

        var player = GameManager.Instance.Player;
        if (player == null || !GodotObject.IsInstanceValid(player)) return;

        float dist = (player.GlobalPosition - GlobalPosition).Length();
        float pickupRadius = player.Stats.Final(StatType.PickupRadius);

        if (dist < pickupRadius)
        {
            float speed = 6f + (pickupRadius - dist) * 6f;
            GlobalPosition += (player.GlobalPosition - GlobalPosition).Normalized() * (float)delta * speed;
        }

        if (dist < CollectDistance)
        {
            GameManager.Instance.LevelUp.AddXp(Value);
            QueueFree();
        }
    }
}
