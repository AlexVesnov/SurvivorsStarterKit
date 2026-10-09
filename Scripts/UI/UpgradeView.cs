using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperheroSurvivors.Core;

/// Экран выбора при левел-апе: 4 карточки, модель Brotato.
/// Класс и путь сохранены для совместимости с HUD.tscn (UpgradeContainer).
public partial class UpgradeView : Control
{
    private PackedScene _choicePanel;

    public override void _Ready()
    {
        _choicePanel = GD.Load<PackedScene>("res://Prefabs/UI/powerup_block.tscn");
        ProcessMode = ProcessModeEnum.Always;
        ClearChoices();
    }

    public Task<LevelUpOption> WaitChoiceAsync(List<LevelUpOption> options)
    {
        var tcs = new TaskCompletionSource<LevelUpOption>();
        ClearChoices();

        foreach (var option in options)
        {
            var panel = _choicePanel.Instantiate<Button>();
            panel.ProcessMode = ProcessModeEnum.Always;

            panel.GetNode<Label>("MarginContainer/VBoxContainer/VBoxPlayer/Name").Text = option.Title;
            panel.GetNode<Label>("MarginContainer/VBoxContainer/VBoxPlayer/Description").Text = option.Description;
            panel.GetNode<Control>("MarginContainer/VBoxContainer/VBoxEnemy").Visible = false;

            var captured = option;
            panel.Pressed += () =>
            {
                foreach (var child in GetChildren())
                    if (child is Button button) button.Disabled = true;
                tcs.TrySetResult(captured);
            };
            AddChild(panel);
        }

        return tcs.Task;
    }

    public void ClearChoices()
    {
        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.QueueFree();
        }
    }
}
