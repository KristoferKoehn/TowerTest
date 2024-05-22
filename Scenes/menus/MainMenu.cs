using Godot;
using System;

public partial class MainMenu : Control
{
    public void _on_play_button_pressed()
    {
        GD.Print("we get here");
        this.GetParent<SceneSwitcher>().PushScene(GD.Load<PackedScene>("res://Scenes/GameLoop/GameLoop.tscn").Instantiate<GameLoop>());

    }

}
