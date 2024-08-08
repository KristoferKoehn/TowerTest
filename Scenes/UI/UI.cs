using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;

public partial class UI : CanvasLayer
{
    [Export]
    Label CurrencyLabel;
    [Export]
    private ProgressBar TowerHealthBar;
    [Export]
    Control GameOverControl;


    public static UI Instance { get; private set; }

    private float[] speedLevels = { 1.0f, 2.0f, 3.0f};
    private int currentSpeedIndex = 0;
    private ScrollContainer _towersPanel;
    private PackedScene _cardScene;
    private GridContainer _gridContainer; // The grid container holding the slots for the tower cards.
    private GameLoop _gameLoop; // Used for placing chunks (could probably be refactored)
 
    private PlayerHand2 _playerHand;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Instance stuff
        if (Instance != null && Instance != this)
        {
            QueueFree();
        }
        else
        {
            Instance = this;
        }

        _playerHand = GetNode<PlayerHand2>("Control/PlayerHand2");
        _gameLoop = GetParent<GameLoop>();
        PlayerStatsManager.GetInstance().StatChanged += GoldUpdate;
        PlayerStatsManager.GetInstance().StatChanged += HealthBarUpdate;
        GoldUpdate(StatType.Gold, PlayerStatsManager.GetInstance().GetStat(StatType.Gold));
        TowerHealthBar.MaxValue = PlayerStatsManager.GetInstance().GetStat(StatType.MaxHealth);
        TowerHealthBar.Value = PlayerStatsManager.GetInstance().GetStat(StatType.Health);

    }
    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (PlayerStatsManager.GetInstance().GetStat(StatType.Health) < 1)
        {
            GameOverControl.Visible = true;
            Tween t = GetTree().CreateTween();
            t.TweenProperty(GameOverControl.GetNode<ColorRect>("ColorRect"), "color", new Color(0, 0, 0, 0.4f), 1.2f);
        }
	}

    public void _on_pause_play_button_pressed()
    {
        if (GetTree().Paused)
        {
            GetTree().Paused = false;
        }
        else
        {
            GetTree().Paused = true;
        }
    }

    public void _on_speed_up_button_pressed()
    {
        if (GetTree().Paused)
        {
            return;
        }

        // Move to the next speed level
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        Engine.TimeScale = speedLevels[currentSpeedIndex];

        GD.Print($"Setting speed to {Engine.TimeScale}x");
    }

    public void _on_settings_button_pressed()
    {
        //this.FindParent("SceneSwitcher").PushScene(GD.Load<PackedScene>("res://Scenes/menus/SettingsMenu.tscn").Instantiate<Control>());
        //pause
        //add child ()
    }

    public void GoldUpdate(StatType type, float value)
    {
        if (type == StatType.Gold)
        {
            CurrencyLabel.Text = $"Currency: {PlayerStatsManager.GetInstance().GetStat(type)}";
        }
    }

    public void HealthBarUpdate(StatType type, float value)
    {
        if (type == StatType.Health)
        {
            TowerHealthBar.Value = PlayerStatsManager.GetInstance().GetStat(type);
        }
    }

    public void _on_chunks_button_pressed()
    {
        _playerHand.ToggleHide();
    }

    public void _on_main_menu_button_pressed()
    {
        SceneSwitcher.Instance.PopScene();
    }
}
