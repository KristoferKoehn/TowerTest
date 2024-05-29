using Godot;
using Godot.Collections;
using System;

public partial class Necromancer : BaseEnemy
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ModelName = "Necromancer";

		Dictionary<StatType, float> sb = new()
		{
			{StatType.Health, 600 },
			{StatType.Speed, 2 },
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
