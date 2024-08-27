using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AudioButton : Button
{
	private AudioStream clickSound;
    private AudioStream hoverSound;
	private AudioStreamPlayer _player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		AddChild(_player);
        clickSound = (AudioStream)GD.Load("res://Assets/Audio/Menus/ps_menu_click.mp3");
        hoverSound = (AudioStream)GD.Load("res://Assets/Audio/Menus/ps_menu_hover.mp3");
		this.MouseEntered += () => PlayHover();
		this.ButtonDown += () => PlayClick();
	}

	private void PlayHover()
	{
		_player.Stream = hoverSound;
		_player.Play();
	}

    private void PlayClick()
    {
        _player.Stream = clickSound;
        _player.Play();
    }
}
