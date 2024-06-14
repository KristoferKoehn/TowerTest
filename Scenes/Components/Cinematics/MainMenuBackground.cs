using Godot;
using System;

[Tool]
public partial class MainMenuBackground : Node3D
{

	[Export]
	float SpinRate = Mathf.Pi / 10;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Node3D>("CameraGimbal").RotateY(SpinRate);


	}
}
