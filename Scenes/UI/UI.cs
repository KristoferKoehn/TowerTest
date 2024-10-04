using Godot;
using Managers;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

public partial class UI : CanvasLayer
{
    [Export] PlayerHand2 PlayerHand;
    [Export] Label CurrencyLabel;
    [Export] Label ScoreLabel;
    [Export] ProgressBar TowerHealthBar;
    [Export] Control GameOverControl;
    [Export] Control PauseControl;

    [Export] Control DeckViewerControl;
    [Export] PanelContainer EffectTimerContainer;

    [Export] Shop ShopScene;


    private float[] fastForwardSpeeds = { 1.0f, 2.0f, 3.0f};
    private int currentFastForwardSpeedIndex = 0;

    private static UI instance;

    public static UI GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            GD.Print("making new UI");
            instance = GD.Load<PackedScene>("res://Scenes/UI/UI.tscn").Instantiate<UI>();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            //instance.ActionName = "UI";
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
        AccountStatsManager.GetInstance().StatChanged += UpdateAccountStatsUI;
        PlayerStatsManager.GetInstance().StatChanged += UpdatePlayerStatsUI;
        ActionManager.GetInstance().TemporaryActionStarted += AddEffectTimeLabel;
        UpdateAccountStatsUI(StatType.Gold, AccountStatsManager.GetInstance().GetStat(StatType.Gold));
        UpdatePlayerStatsUI(StatType.Score, PlayerStatsManager.GetInstance().GetStat(StatType.Score));
        TowerHealthBar.MaxValue = PlayerStatsManager.GetInstance().GetStat(StatType.MaxHealth);
        TowerHealthBar.Value = PlayerStatsManager.GetInstance().GetStat(StatType.Health);
        this.GameOverControl.Visible = false;
        this.DeckViewerControl.Visible = false;
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

    // click to pause and click again to unpause
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
        Label speedUpLabel = GetNode<Label>("Control/VBoxContainer/TopUI/Buttons_HBoxContainer/SpeedUpButton/SpeedUp");
        // Move to the next speed level
        currentFastForwardSpeedIndex = (currentFastForwardSpeedIndex + 1) % fastForwardSpeeds.Length;
        speedUpLabel.Text = (currentFastForwardSpeedIndex + 1) + "x";

        Engine.TimeScale = fastForwardSpeeds[currentFastForwardSpeedIndex];
        //SetTowersAndEnemyTimeScale();
        // That one complicates stuff so its better to just have everything move faster. We'd
        // have to put a time scale in everything that we want to move faster (enemies, towers, status effects, etc.)

        GD.Print($"Setting speed to {currentFastForwardSpeedIndex + 1}x");
    }

    // Use this to avoid the Engine.TimeScale setting, but honestly it makes things
    // more complicated, so i don't think we should use it.
    private void SetTowersAndEnemyTimeScale()
    {
        float timeScale = fastForwardSpeeds[currentFastForwardSpeedIndex];
        foreach (BaseEnemy enemy in EnemyManager.GetInstance().Enemies)
        {
            enemy.TimeScale = timeScale;
        }
        foreach (List<AbstractTower> towerList in TowerManager.GetInstance().activeTowers.Values)
        {
            foreach (AbstractTower tower in towerList)
            {
                tower.TimeScale = timeScale;
            }
        }

        TowerManager.GetInstance().CurrentTowerTimeScale = timeScale;
        EnemyManager.GetInstance().CurrentEnemyTimeScale = timeScale;
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
        Engine.TimeScale = 1;
        AudioManager.GetInstance().PlayMenuSwitch();
        SceneSwitcher.Instance.PopScene(); // get rid of the current game loop.
    }

    public void _on_begin_wave_button_pressed()
    {
        WaveManager.GetInstance().StartWave();
    }

    public void _on_deck_button_pressed()
    {
        this.DeckViewerControl.Visible = !this.DeckViewerControl.Visible;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("DiscardHand"))
        {
            
        }

        if (@event.IsActionPressed("DrawCards"))
        {

        }
    }


    public void AddEffectTimeLabel(BaseAction action)
    {
        GD.Print(action.IsTemporary);
        if (action.IsTemporary)
        {
            action.durationTimerLabel.Reparent(EffectTimerContainer);
        }
    }
    
    public void _on_shop_button_pressed()
    {
        PlayerHand.HideButtons();
        PlayerHand.ForceHandDown(false);
        ShopScene.Visible = true;
    }

    public void ShopClosed()
    {
        PlayerHand.ShowButtons();
        PlayerHand.ForceHandUp(false);
    }
}
