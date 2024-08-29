using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class Shop : Control
{

    [Export] PackedScene CardScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{


    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public void OnChunkCardClicked(InputEvent @event, TextureRect slot)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GD.Print("Clicked on a chunk card.");
        }
    }
}
