using Godot;
using System;

public partial class MainMenu : Control
{
    public void _on_play_button_pressed()
    {
        this.GetParent<SceneSwitcher>().PushScene("res://Scenes/GameLoop/GameLoop.tscn");
    }

    public void _on_quit_button_pressed()
    {
        GetTree().Quit();
    }

}
