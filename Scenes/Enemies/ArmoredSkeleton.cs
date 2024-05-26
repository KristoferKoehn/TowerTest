using Godot;
using Godot.Collections;
using System;

public partial class ArmoredSkeleton : BaseEnemy
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        ModelName = "Skeleton_Warrior";

        Dictionary<StatType, float> sb = new()
        {
            {StatType.Health, 100 },
            {StatType.Speed, 2 },
            {StatType.Armor, 500 },
        };
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        base._Process(delta);
    }
}
