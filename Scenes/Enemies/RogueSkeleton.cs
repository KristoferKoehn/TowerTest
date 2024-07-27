using Godot;
using System;
using System.Collections.Generic;

public partial class RogueSkeleton : BaseEnemy
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Dictionary<StatType, float> sb = new()
		{
			{StatType.Health, 100 },
			{StatType.Speed, 300 },
			{StatType.Armor, 0 },
			{ StatType.FireResist, 0},
			{StatType.Damage,  100}
		};

		StatBlock.SetStatBlock(sb);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
