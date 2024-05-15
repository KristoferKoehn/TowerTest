using Godot;
using System;
using System.Collections.Generic;

public partial class Chunk : Node3D
{

    [Export]
    bool NorthEntrance = false;

    [Export]
    bool EastEntrance = false;

    [Export]
    bool WestEntrance = false;

    [Export]
    bool SouthEntrance = false;

    //hold onto the tiles or something
    List<Path3D> Paths;

    public void PlaceChunk()
    {
        //find all lane tiles

        //go from one entrance to the other

        foreach(Node child in this.GetChildren()) 
        { 
            MeshInstance3D mesh = child as MeshInstance3D;

            if (mesh != null)
            {
                if (mesh.GetMeta("height").AsInt32() == 0)
                {

                }
            }

        }

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
