using Godot;
using System;

public partial class ChunkSwap : StaticBody3D
{
	[Export]
	Chunk swap1 { get; set; }

	[Export]
	Chunk swap2 { get; set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public void _on_input_event(Camera3D node, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
	{
		if (@event.IsActionPressed("select"))
		{
			//check if swap1 and swap2 have the same exit cardinality
			//if not, save swap 1 cardinality, rotate swap1 until == swap2 cardinality
			//rotate swap2 until == tempcard



        }
		
	}

}
