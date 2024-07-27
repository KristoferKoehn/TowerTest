using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;

public partial class GolemSkeleton : BaseEnemy
{
	//ModelName = "Skeleton_Golem";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Dictionary<StatType, float> sb = new()
		{
			{StatType.Health, 500 },
			{StatType.Speed, 2 },
			{StatType.Armor, 100 },
			{StatType.FireResist, 0},
			{StatType.Damage,  500}
		};

		StatBlock = new StatBlock();
		StatBlock.SetStatBlock(sb);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
