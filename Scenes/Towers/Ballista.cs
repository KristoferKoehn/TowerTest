using Godot;
using System;

public partial class Ballista : Node3D
{
	MeshInstance3D BallistaMount;
	MeshInstance3D BallistaBow;
    MeshInstance3D Arrow;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		BallistaMount = GetNode<MeshInstance3D>("weapon_ballista2");
        BallistaBow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow");
		Arrow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow");

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
