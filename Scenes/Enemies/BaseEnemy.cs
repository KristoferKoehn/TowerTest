using Godot;
using System;

public partial class BaseEnemy : PathFollow3D
{

	//stats, health whatever
	//speed

	//a spawner somewhere (not here)


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (ProgressRatio == 1)
		{
            //switch paths
            //raycasting down (to hit the tile), asking for it's parent, then passing the tile into the chunk's thingy (GetPathsFromEntrance)
			//reparent to new path, set progress ratio to 0
        }
    }
}
