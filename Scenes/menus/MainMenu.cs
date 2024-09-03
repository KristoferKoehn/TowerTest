using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] AudioStreamPlayer2D MouseOver;
    [Export] AudioStreamPlayer2D Press;


    public void _on_play_button_pressed()
    {
        this.GetParent<SceneSwitcher>().PushScene("res://Scenes/GameLoop/GameLoop.tscn");
    }

    public void _on_quit_button_pressed()
    {
        GetTree().Quit();
    }

    public void PlayMouseOverSound()
    {
        MouseOver.Play();
    }

    public void PlayPressSound()
    {
        Press.Play();
    }

}
