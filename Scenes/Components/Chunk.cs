using Godot;
using System;
using System.Collections.Generic;

public partial class Chunk : Node3D
{

    //hold onto the tiles or something
    List<Path3D> Paths;

    public void PlaceChunk()
    {
        //find all lane tiles

        //go from one entrance to the other


    }


    //called when moving between chunks?
    public Path3D GetNextPath()
    {
        return new Path3D();
    }


    public override void _Ready()
    {

    }

    public override void _Process(double delta)
    {

    }



}
