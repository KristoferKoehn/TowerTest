using Godot;
using System;
using System.Collections.Generic;

public partial class Necromancer : BaseEnemy
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		base._Ready();
        StrikeSound = GetNode<AudioStreamPlayer3D>("StrikeSound");

		ModelName = "Necromancer";

        Dictionary<StatType, float> sb = new()
        {
            {StatType.Health, 600 },
            {StatType.Speed, 2 },
        };
        StatBlock.SetStatBlock(sb);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
