using Godot;
using System;

public partial class MainMenu : Control
{
    public void _on_play_button_pressed()
    {
        this.GetParent<SceneSwitcher>().PushScene(GD.Load<PackedScene>("res://Scenes/GameLoop/GameLoop.tscn").Instantiate<GameLoop>());

    }

    public void _on_quit_button_pressed()
    {
        GetTree().Quit();
    }

}
