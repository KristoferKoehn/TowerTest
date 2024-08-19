using Godot;
using Godot.Collections;
using Managers;
using System;

public partial class WaveDataManager : Node
{

    private static WaveDataManager instance;

    private WaveDataManager() { }

    public static WaveDataManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new WaveDataManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "WaveDataManager";
        }
        return instance;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{



	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public Array<Array<Array<string>>> LoadWave(string WaveName)
    {
        string serialized = Load(WaveName);

        Variant v = Json.ParseString(serialized);
        return v.AsGodotArray<Array<Array<string>>>();

    }

    public string Load(string WaveFile)
    {
        using var file = FileAccess.Open($"res://Assets/WaveData/{WaveFile}", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return null;
        }
        string content = file.GetAsText();
        return content;
    }

}
