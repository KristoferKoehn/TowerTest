using Godot;

public partial class UI : CanvasLayer
{
    [Export] PlayerHand2 _playerHand;
    [Export] Label CurrencyLabel;
    [Export] Label ScoreLabel;
    [Export] ProgressBar TowerHealthBar;
    [Export] Control GameOverControl;
    [Export] private Control PauseControl;

    private float[] fastForwardSpeeds = { 1.0f, 2.0f, 3.0f};
    private int currentFastForwardSpeedIndex = 0;
    private GameLoop _gameLoop; // Used for placing chunks (could probably be refactored)

    private static UI instance;

    public static UI GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            GD.Print("making new UI");
            instance = GD.Load<PackedScene>("res://Scenes/UI/UI.tscn").Instantiate<UI>();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            //instance.ArtifactName = "UI";
        }
        return instance;
    }

    public override void _ExitTree()
    {
        // Ensure that any connected signals are disconnected when the UI is freed
        AccountStatsManager.GetInstance().StatChanged -= UpdateAccountStatsUI;
        PlayerStatsManager.GetInstance().StatChanged -= UpdatePlayerStatsUI;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _gameLoop = GetParent<GameLoop>();
        AccountStatsManager.GetInstance().StatChanged += UpdateAccountStatsUI;
        PlayerStatsManager.GetInstance().StatChanged += UpdatePlayerStatsUI;
        UpdateAccountStatsUI(StatType.Gold, AccountStatsManager.GetInstance().GetStat(StatType.Gold));
        UpdatePlayerStatsUI(StatType.Score, PlayerStatsManager.GetInstance().GetStat(StatType.Score));
        TowerHealthBar.MaxValue = PlayerStatsManager.GetInstance().GetStat(StatType.MaxHealth);
        TowerHealthBar.Value = PlayerStatsManager.GetInstance().GetStat(StatType.Health);
        this.GameOverControl.Visible = false;
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
        GetTree().Paused = !GetTree().Paused;
        this.PauseControl.Visible = !this.PauseControl.Visible;
    }

    public void _on_speed_up_button_pressed()
    {
        if (GetTree().Paused)
        {
            return;
        }

        // Move to the next speed level
        currentFastForwardSpeedIndex = (currentFastForwardSpeedIndex + 1) % fastForwardSpeeds.Length;
        Engine.TimeScale = fastForwardSpeeds[currentFastForwardSpeedIndex];

        GD.Print($"Setting speed to {Engine.TimeScale}x");
    }

    public void _on_settings_button_pressed()
    {

    }

    public void UpdateAccountStatsUI(StatType updatedStat, float newVal)
    {
        switch (updatedStat)
        {
            case StatType.Gold:
                CurrencyLabel.Text = $"Currency: {newVal}";
                break;
        }
    }

    public void UpdatePlayerStatsUI(StatType updatedStat, float newVal)
    {
        switch (updatedStat)
        {
            case StatType.Score:
                ScoreLabel.Text = $"Score: {newVal}";
                break;
            case StatType.Health:
                TowerHealthBar.Value = newVal;
                break;
        }
    }

    public void _on_main_menu_button_pressed()
    {
        //PlayerStatsManager.GetInstance().SetStat(StatType.Health, PlayerStatsManager.GetInstance().GetStat(StatType.MaxHealth));

        SceneSwitcher.Instance.PopScene(); // get rid of the current game loop.
    }
}
