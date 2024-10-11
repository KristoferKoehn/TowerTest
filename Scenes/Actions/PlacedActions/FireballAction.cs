using Godot;
using System;
using TowerTest.Scenes.Components;

public partial class FireballAction : AbstractPlaceable
{

    [Export] MeshInstance3D CursorOutline;
    [Export] Node3D DisplayFireball;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void DisplayMode()
    {
        throw new NotImplementedException();
    }

    public override void ActivatePlacing()
    {
        throw new NotImplementedException();
    }
}
