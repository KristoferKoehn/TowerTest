using Godot;
using System;

using Godot;

public partial class AudioManager : Node
{
    public static AudioManager instance;

    private AudioStream menuChangeSound;

    public AudioStreamPlayer player;

    public static AudioManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new AudioManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "AudioManager";
        }
        return instance;
    }

    public override void _Ready()
    {
        menuChangeSound = (AudioStream)GD.Load("res://Assets/Audio/Menus/menu_change.mp3");

        player = new AudioStreamPlayer();
        AddChild(player);
    }

    public void PlayMenuSwitch()
    {
        player.Stream = menuChangeSound;
        player.Play();
    }
}

