using Godot;
using System;
using System.Collections.Generic;


public enum Direction
{
    North,
    East,
    South,
    West
}

[Tool]
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

    [Export]
    bool RotateCW = false;
    [Export]
    bool RotateCCW = false;

    MeshInstance3D NorthTile = null;
    MeshInstance3D EastTile = null;
    MeshInstance3D WestTile = null;
    MeshInstance3D SouthTile = null;

    int ChunkRotation = 0;

    public override void _Ready()
    {
        if(NorthEntrance)
        {
            MeshInstance3D temp = GetNode<MeshInstance3D>("0, -3");
            if(temp.GetMeta("height").AsInt32() == 0)
            {
                NorthTile = temp;
            }
        }
        if(EastEntrance)
        {
            MeshInstance3D temp = GetNode<MeshInstance3D>("0, -3");
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                EastTile = temp; 
            }
        }
        if(WestEntrance)
        {
            MeshInstance3D temp = GetNode<MeshInstance3D>("0, -3");
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                WestTile = temp;
            }
        }
        if(SouthEntrance)
        {
            MeshInstance3D temp = GetNode<MeshInstance3D>("0, -3");
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                SouthTile = temp;
            }
        }
    }

    //hold onto the tiles or something
    List<Path3D> Paths;

    public void PlaceChunk(Direction ConnectedDirection)
    {



        //find all lane tiles

        //go from one entrance to the other

    }


    //called when moving between chunks?
    public Path3D GetNextPath()
    {
        return new Path3D();
    }


    public void RotateClockwise()
    {
        bool temp = NorthEntrance;
        NorthEntrance = WestEntrance;
        WestEntrance = SouthEntrance;
        SouthEntrance = EastEntrance;
        EastEntrance = temp;

        MeshInstance3D tempTile = NorthTile;
        NorthTile = WestTile;
        WestTile = SouthTile;
        SouthTile = EastTile;
        EastTile = tempTile;

        ChunkRotation++;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "rotation", new Vector3(0, (float)-Math.PI / 2f * ChunkRotation, 0), 0.2f);

    }

    public void RotateCounterClockwise()
    {
        bool temp = NorthEntrance;
        NorthEntrance = EastEntrance;
        EastEntrance = SouthEntrance;
        SouthEntrance = WestEntrance;
        WestEntrance = temp;

        MeshInstance3D tempTile = NorthTile;
        NorthTile = EastTile;
        EastTile = SouthTile;
        SouthTile = WestTile;
        WestTile = tempTile;

        ChunkRotation--;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "rotation", new Vector3(0, (float)-Math.PI / 2f * ChunkRotation, 0), 0.2f);

    }



    public override void _Process(double delta)
    {
        if (RotateCW)
        {
            RotateCW = false;
            RotateClockwise();
        }

        if (RotateCCW)
        {
            RotateCCW = false;
            RotateCounterClockwise();
        }
    }



}
