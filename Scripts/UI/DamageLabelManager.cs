using Godot;
using System.Collections.Generic;
using SuperheroSurvivors.Core;

/// Корень HUD + пул вылетающих цифр урона.
/// Скрипт висит на корне HUD (instance в main_scene.tscn) — пути относительно него.
public partial class DamageLabelManager : Control
{
    public const uint InitialQueue = 50;

    private readonly Queue<Label> _queue = new();
    private readonly List<Label> _displayedLabels = new();

    private Camera3D _camera;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _camera = GetNode<Camera3D>("../Player/Camera3D");
        GameEvents.EnemyDamaged += OnEnemyDamaged;

        for (uint i = 0; i < InitialQueue; ++i)
            _queue.Enqueue(NewPooledLabel());
    }

    private static Label NewPooledLabel() => new() { ProcessMode = ProcessModeEnum.Always };

    public Label GetLabel()
    {
        var label = _queue.Count == 0 ? NewPooledLabel() : _queue.Dequeue();
        label.Modulate = new Color(1, 1, 1, 1);
        _displayedLabels.Add(label);
        AddChild(label);
        return label;
    }

    public void ReturnToPool(Label label)
    {
        _queue.Enqueue(label);
        _displayedLabels.Remove(label);
        RemoveChild(label);
    }

    private void OnEnemyDamaged(Enemy enemy, DamageInfo info)
    {
        if (!GodotObject.IsInstanceValid(enemy)) return;

        var label = GetLabel();
        label.Text = Mathf.RoundToInt(info.Amount).ToString();
        label.LabelSettings = new LabelSettings
        {
            FontSize = info.IsCrit ? 42 : 26,
            FontColor = info.IsCrit ? new Color(1f, 0.9f, 0.2f) : ColorFor(info.Type),
            OutlineSize = 4,
            OutlineColor = new Color(0, 0, 0),
        };

        Vector2 labelPos = _camera.UnprojectPosition(enemy.GlobalPosition);
        labelPos.X -= label.Size.X / 2;
        labelPos.Y -= 50;
        label.Position = labelPos;

        var tween = CreateTween();
        tween.TweenProperty(label, "position:y", labelPos.Y - 50, 0.75f);
        tween.Parallel().TweenProperty(label, "modulate", new Color(1, 1, 1, 0), 0.5f).SetDelay(0.25f);
        tween.TweenCallback(Callable.From(() => ReturnToPool(label)));
    }

    private static Color ColorFor(DamageType type) => type switch
    {
        DamageType.Fire => new Color(1f, 0.55f, 0.2f),
        DamageType.Tech => new Color(0.4f, 0.85f, 1f),
        _ => new Color(1f, 1f, 1f),
    };
}
