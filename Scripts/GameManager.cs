using Godot;
using SuperheroSurvivors.Core;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    public Player Player { get; set; }
    public double GameTime { get; private set; } = 0;
    public bool IsLevelUpActive { get; set; }

    private EnemyManager _enemyManager;
    private LevelUpSystem _levelUp;
    private PackedScene _xpGemScene;

    private double _spawnTimer;
    private Label _gameTimeLabel;
    private ProgressBar _playerXpBar;
    private ProgressBar _playerLifeBar;

    public LevelUpSystem LevelUp => _levelUp;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        _enemyManager = new EnemyManager(this);
        _xpGemScene = GD.Load<PackedScene>("res://Prefabs/Progression/xp_gem.tscn");

        _gameTimeLabel = GetNode<Label>("/root/MainScene/HUD/GameTime");
        _playerXpBar = GetNode<ProgressBar>("/root/MainScene/HUD/PlayerXPBar");
        _playerLifeBar = GetNode<ProgressBar>("/root/MainScene/HUD/PlayerLifeBar");

        var upgradeView = GetNode<UpgradeView>("/root/MainScene/HUD/UpgradeContainer");
        _levelUp = new LevelUpSystem(this, upgradeView);

        GameEvents.XpChanged += OnXpChanged;
        GameEvents.LevelChanged += OnLevelChanged;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GetTree().Paused) return;

        // Debug: кнопка спавна босса из Input Map проекта
        if (Input.IsActionJustPressed("SpawnBoss"))
            _enemyManager.SpawnBoss(GameTime);

        GameTime += delta;
        _gameTimeLabel.Text = $"{Mathf.FloorToInt(GameTime / 60):00}:{Mathf.FloorToInt(GameTime % 60):00}";

        _enemyManager.TickSeparation();

        _spawnTimer -= delta;
        if (_spawnTimer > 0 || _enemyManager.Enemies.Count >= EnemyManager.MaxEnemies) return;

        _spawnTimer = _enemyManager.SpawnInterval(GameTime);
        int batch = _enemyManager.SpawnBatch(GameTime);
        for (int i = 0; i < batch; i++)
            _enemyManager.SpawnEnemy(GameTime);
    }

    internal void OnEnemyKilled(Enemy enemy)
    {
        var gem = _xpGemScene.Instantiate<XpGem>();
        gem.Value = enemy.Experience;
        gem.GlobalPosition = enemy.GlobalPosition;
        GetNode("/root/MainScene").AddChild(gem);
    }

    public Enemy NearestEnemy(Vector3 from) => _enemyManager.Nearest(from);

    public Vector3 GetRandomPosAroundPlayer(float range) => Player.Position + range * new Vector3(
        (float)GD.RandRange(-1f, 1f),
        0,
        (float)GD.RandRange(-1f, 1f)
    ).Normalized();

    private void OnXpChanged(float current, float needed)
    {
        _playerXpBar.MaxValue = needed;
        _playerXpBar.Value = current;
    }

    private int _level = 1;
    private void OnLevelChanged(int level)
    {
        _level = level;
        UpdateLifeBar();
    }

    public void UpdateLifeBar()
    {
        if (Player == null) return;
        _playerLifeBar.MaxValue = Player.Stats.Final(StatType.MaxHealth);
        _playerLifeBar.Value = Player.CurrentHealth;
    }
}
