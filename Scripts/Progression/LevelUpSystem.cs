using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperheroSurvivors.Core;

/// Опыт и уровни героя внутри забега.
/// Левел-ап = пауза + выбор 1 из 4 (модель Brotato). Голосования кита удалены.
public class LevelUpSystem
{
    public int Level { get; private set; } = 1;

    private readonly GameManager _gameManager;
    private readonly UpgradeView _view;
    private float _xp;
    private float _needed;

    public LevelUpSystem(GameManager gameManager, UpgradeView view)
    {
        _gameManager = gameManager;
        _view = view;
        _needed = XpForLevel(Level);
        GameEvents.RaiseXpChanged(0, _needed);
    }

    /// Кривая опыта из кита: 5 XP на 1-м уровне, ~20 на 2-м, ~29 на 3-м, ~55 на 10-м.
    public static float XpForLevel(int level) => 50f * (float)Math.Log10(level) + 5f;

    public void AddXp(float value)
    {
        if (_gameManager.IsLevelUpActive) return;

        _xp += value;
        GameEvents.RaiseXpChanged(_xp, _needed);

        if (_xp >= _needed)
            _ = StartLevelUp();
    }

    private async Task StartLevelUp()
    {
        _gameManager.IsLevelUpActive = true;
        _gameManager.GetTree().Paused = true;

        var options = UpgradePool.Roll(_gameManager.Player, 4);
        var picked = await _view.WaitChoiceAsync(options);

        picked.Apply(_gameManager.Player);

        Level++;
        _xp = 0;
        _needed = XpForLevel(Level);
        GameEvents.RaiseLevelChanged(Level);
        GameEvents.RaiseXpChanged(_xp, _needed);
        _gameManager.UpdateLifeBar();

        _view.ClearChoices();
        _gameManager.GetTree().Paused = false;
        _gameManager.IsLevelUpActive = false;

        // Если опыт с гемов превысил порог следующего уровня — цепочка продолжается
        if (_xp >= _needed)
            _ = StartLevelUp();
    }
}
