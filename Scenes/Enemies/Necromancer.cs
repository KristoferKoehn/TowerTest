using Godot;
using System;
using System.Collections.Generic;

public partial class Necromancer : BaseEnemy
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

        Dictionary<StatType, float> sb = new()
        {
            {StatType.Health, 600 },
            {StatType.Speed, 1 },
            {StatType.Gold, 4 },
            {StatType.Damage, 10 },

			// Damage Multipliers:
			{StatType.PhysicalMultiplier, 1 },
            {StatType.FireMultiplier, 1 },
            {StatType.IceMultiplier, 1 },
            {StatType.WindMultiplier, 1 },
            {StatType.WaterMultiplier, 1 },
            {StatType.ShockMultiplier, 1 },
            {StatType.PoisonMultiplier, 1 },
        };
        StatBlock.SetStatBlock(sb);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
