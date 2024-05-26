using Godot;
using System;

public partial class Skeleton : BaseEnemy
{
	Health = 100;
	Speed = 3; // in m/s
	Armor = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
