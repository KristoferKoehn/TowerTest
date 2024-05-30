using Godot;
using Godot.Collections;
using System;

public partial class Skeleton : BaseEnemy
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ModelName = "Skeleton_Minion";

		Dictionary<StatType, float> sb = new()
		{
			{StatType.Health, 100 },
			{StatType.Speed, 3 },
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//Anything for the enemy before the process of moving. 
		base._Process(delta);
	}
}
