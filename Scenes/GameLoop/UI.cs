using Godot;
using System;

public partial class UI : CanvasLayer
{

    private float[] speedLevels = { 1.0f, 2.0f, 3.0f};
    private int currentSpeedIndex = 0;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void _on_pause_play_button_pressed()
    {
        if (GetTree().Paused)
        {
            GetTree().Paused = false;
            GD.Print("Resuming");
        }
        else
        {
            GetTree().Paused = true;
            GD.Print("Pausing");
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
    }
}
