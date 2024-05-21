using Godot;
using Godot.Collections;
using System;

public enum Direction
{
    North,
    East,
    South,
    West,
    Down
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
    MeshInstance3D ExitTile = null;

    [Export]
    bool RotateCW = false;
    [Export]
    bool RotateCCW = false;
    [Export]
    bool GeneratePath = false;

    [Export]
    MeshInstance3D NorthTile = null;
    [Export]
    MeshInstance3D EastTile = null;
    [Export]
    MeshInstance3D WestTile = null;
    [Export]
    MeshInstance3D SouthTile = null;

    //this is so we know how big the chunk is. For later.
    public int ChunkSize = 7;


    //this is for tweening.
    int ChunkRotation = 0;


    Dictionary<MeshInstance3D, Array<MeshInstance3D>> AdjacencyList = new();
    Array<MeshInstance3D> AllLaneTiles = new();

    public Array<Path3D> AllLanePaths = new();



    /// <summary>
    /// Raycasts 1m out from origin in direction. if Direction.Down is supplied, it will raycast downward.
    /// </summary>
    /// <param name="direction">direction of raycast from given origin, type Direction</param>
    /// <param name="origin">Vector3 Origin of raycast</param>
    /// <returns></returns>
    public MeshInstance3D GetAdjacentTile(Direction direction, Vector3 origin)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        PhysicsRayQueryParameters3D query = null;
        if (direction == Direction.North)
        {
            query = PhysicsRayQueryParameters3D.Create(origin, origin + new Vector3(0, 0, -1), collisionMask: 8);
        }
        if (direction == Direction.West)
        {
            query = PhysicsRayQueryParameters3D.Create(origin, origin + new Vector3(-1, 0, 0), collisionMask: 8);
        }
        if (direction == Direction.South)
        {
            query = PhysicsRayQueryParameters3D.Create(origin, origin + new Vector3(0, 0, 1), collisionMask: 8);
        }
        if (direction == Direction.East)
        {
            query = PhysicsRayQueryParameters3D.Create(origin, origin + new Vector3(1, 0, 0), collisionMask: 8);
        }
        if (direction == Direction.Down)
        {
            query = PhysicsRayQueryParameters3D.Create(origin, origin + new Vector3(0, -1, 0), collisionMask: 8);
        }


        Dictionary result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            return ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
        }
        else
        {
            return null;
        }
    }


    public override void _Ready()
    {
        UpdateEntrances();
        UpdateAdjacencyList();
    }

    public void UpdateAdjacencyList()
    {
        AllLaneTiles.Clear();
        AdjacencyList.Clear();



        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                MeshInstance3D tile = GetAdjacentTile(Direction.Down, Position + new Vector3(Position.X - i + ChunkSize / 2, Position.Y + 1, Position.Z - j + ChunkSize / 2));
                if (tile.GetMeta("height").AsInt32() == 0)
                {

                    AllLaneTiles.Add(tile);

                    if (!AdjacencyList.ContainsKey(tile))
                    {
                        AdjacencyList.Add(tile, new Array<MeshInstance3D>());
                    }

                    foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                    {
                        MeshInstance3D temp = GetAdjacentTile(dir, tile.GlobalPosition);

                        //check to see if this tile exists and also is related to this chunk
                        //don't want no bs with unrelated tiles. might get weird.
                        if (temp != null && this.IsAncestorOf(temp) && temp.GetMeta("height").AsInt32() == 0)
                        {
                            AdjacencyList[tile].Add(temp);
                        }
                    }
                }
            }
        }
    }

    public void UpdateEntrances()
    {

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(0, 2, -3), GlobalPosition + new Vector3(0, 0, -3), collisionMask: 8);
        Dictionary result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                NorthTile = temp;
                NorthEntrance = true;
            }
            else
            {
                NorthTile = null;
                NorthEntrance = false;
            }
        }
        else
        {
            NorthTile = null;
        }


        query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(3, 2, 0), GlobalPosition + new Vector3(3, 0, 0), collisionMask: 8);
        result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                EastTile = temp;
                EastEntrance = true;
            }
            else
            {
                EastTile = null;
                EastEntrance = false;
            }
        }
        else
        {
            EastTile = null;
        }


        query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(-3, 2, 0), GlobalPosition + new Vector3(-3, 0, 0), collisionMask: 8);
        result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                WestTile = temp;
                WestEntrance = true;
            } else
            {
                WestTile = null;
                WestEntrance = false;
            }
            
        }
        else
        {
            WestTile = null;
        }

        query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(0, 2, 3), GlobalPosition + new Vector3(0, 0, 3), collisionMask: 8);
        result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                SouthTile = temp;
                SouthEntrance = true;
            }
            else
            {
                SouthTile = null;
                SouthEntrance = false;
            }
        }
        else
        {
            SouthTile = null;
        }
    }


    public static Array<Array<MeshInstance3D>> FindAllPaths(Dictionary<MeshInstance3D, Array<MeshInstance3D>> graph, MeshInstance3D startVertex, MeshInstance3D endVertex)
    {
        Array<Array<MeshInstance3D>> allPaths = new Array<Array<MeshInstance3D>>();
        Dictionary<MeshInstance3D, bool> visited = new();
        foreach (MeshInstance3D key in graph.Keys)
        {
            visited[key] = false;
        }


        Array<MeshInstance3D> currentPath = new Array<MeshInstance3D>();
        DFS(graph, startVertex, endVertex, visited, currentPath, allPaths);
        return allPaths;
    }


    private static void DFS(Dictionary<MeshInstance3D, Array<MeshInstance3D>> graph, MeshInstance3D current, MeshInstance3D end, Dictionary<MeshInstance3D, bool> visited, Array<MeshInstance3D> currentPath, Array<Array<MeshInstance3D>> allPaths)
    {
        visited[current] = true;
        currentPath.Add(current);

        if (current == end)
        {
            allPaths.Add(new Array<MeshInstance3D>(currentPath));
        }
        else
        {
            if(!graph.ContainsKey(current))
            {
                GD.Print(current.Name);
            }
            foreach (MeshInstance3D neighbor in graph[current])
            {
                if (!visited[neighbor])
                {
                    DFS(graph, neighbor, end, visited, currentPath, allPaths);
                }
            }
        }

        currentPath.RemoveAt(currentPath.Count - 1);
        visited[current] = false;
    }

   

    public void PlaceChunk(Direction ConnectedDirection)
    {

        //find all lane tiles

        //go from one entrance to the other

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

    public void CreatePaths(MeshInstance3D Entrance)
    {
        Array<Array<MeshInstance3D>> allPaths = FindAllPaths(AdjacencyList, NorthTile, ExitTile);


        foreach (Array<MeshInstance3D> path in allPaths)
        {
            Path3D temp = new Path3D();
            temp.Curve = new Curve3D();
            AllLanePaths.Add(temp);

            foreach (MeshInstance3D mesh in path)
            {
                temp.Curve.AddPoint(mesh.Position + new Vector3(0, 1, 0));
            }

            if (Engine.IsEditorHint())
            {
                GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(temp);
                temp.Owner = GetTree().EditedSceneRoot;

            }
            else
            {
                AddChild(temp);
            }
        }
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

        if (GeneratePath)
        {
            GeneratePath = false;

            foreach(Path3D p in AllLanePaths)
            {
                if (p != null)
                {
                    p.QueueFree();
                }
            }

            AllLanePaths.Clear();

            if(NorthEntrance)
            {
                GD.Print("making paths from " + NorthTile.Name + " To " + ExitTile.Name);
                CreatePaths(NorthTile);
            }
            if (SouthEntrance)
            {
                GD.Print("making paths from " + SouthTile.Name + " To " + ExitTile.Name);
                CreatePaths(SouthTile);
            }
            if (EastEntrance)
            {
                GD.Print("making paths from " + EastTile.Name + " To " + ExitTile.Name);
                CreatePaths(EastTile);
            }
            if (WestEntrance)
            {
                GD.Print("making paths from " + WestTile.Name + " To " + ExitTile.Name);
                CreatePaths(WestTile);
            }
        }
    }
}
