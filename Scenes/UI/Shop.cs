using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class Shop : Control
{
    //either through UI or this, make shop inaccessible during wave
    //on end of wave, randomize cards using some heuristic later.
    //differentiate common/uncommon and rare/epic/legendary slots.
    //we can dynamically add card slots that's not a big deal. Maybe for upgrading the shop somehow



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
