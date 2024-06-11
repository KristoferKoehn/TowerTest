using Godot;
using System;

public partial class ParticleSignals : Node
{
	//use this class to ping when an effect needs to happen. Anyone that might need to fire particles should have a refference to this singleton fellow.
	private static ParticleSignals instance;
	private ParticleSignals() { }

	public static ParticleSignals GetInstance()
	{
		if (instance == null)
		{
			instance = new ParticleSignals();
			SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
			instance.Name = "ParticleSignals";
		}
		return instance;
	}

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	// Create as many of these as we end up having particle effects.
	[Signal]
	public delegate void p1EventHandler(Vector3 position, Vector3 degreeRotation);


}
