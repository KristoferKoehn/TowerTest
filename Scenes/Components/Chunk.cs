using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Security.Cryptography;

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
    bool CenterEntrance = false;

    [Export]
    Direction ExitDirection = Direction.North;

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
    [Export]
    MeshInstance3D CenterTile = null;


    [Export]
    bool QueryValid = false;
    [Export]
    bool QueryInvalid = false;
    [Export]
    bool ClearOverrides = false;




    //this is so we know how big the chunk is. For later.
    public int ChunkSize = 7;


    //this is for tweening.
    int ChunkRotation = 0;


    Dictionary<MeshInstance3D, Array<MeshInstance3D>> AdjacencyList = new();
    Array<MeshInstance3D> AllLaneTiles = new();

    public Array<Path3D> AllLanePaths = new();

    Dictionary<MeshInstance3D, Array<Path3D>> EntrancePathList = new();

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

    public void UpdateAdjacencyList()
    {
        AllLaneTiles.Clear();
        AdjacencyList.Clear();

        for (int i = 0; i < ChunkSize; i++)
        {
            for (int j = 0; j < ChunkSize; j++)
            {
                
                MeshInstance3D tile = GetAdjacentTile(Direction.Down, new Vector3(GlobalPosition.X - i + ChunkSize / 2, GlobalPosition.Y + 1, GlobalPosition.Z - j + ChunkSize / 2));
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

        query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(0, 2, 0), GlobalPosition + new Vector3(0, 0, 0), collisionMask: 8);
        result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            if (temp.GetMeta("height").AsInt32() == 0)
            {
                CenterTile = temp;
                CenterEntrance = true;
            }
            else
            {
                CenterTile = null;
                CenterEntrance = false;
            }
        }
        else
        {
            CenterTile = null;
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

        ExitDirection = ConnectedDirection;

        UpdateEntrances();

        foreach (Path3D p in AllLanePaths)
        {
            if (p != null)
            {
                p.QueueFree();
            }
        }

        AllLanePaths.Clear();

        if (NorthEntrance && ExitDirection != Direction.North)
        {
            CreatePaths(NorthTile);
        }
        if (SouthEntrance && ExitDirection != Direction.South)
        {
            CreatePaths(SouthTile);
        }
        if (EastEntrance && ExitDirection != Direction.East)
        {
            CreatePaths(EastTile);
        }
        if (WestEntrance && ExitDirection != Direction.West)
        {
            CreatePaths(WestTile);
        }
        if (CenterEntrance && ExitDirection != Direction.Down)
        {
            CreatePaths(CenterTile);
        }
    }

    public void RotateClockwise()
    {
        Quaternion q = new Quaternion(Vector3.Up, -Mathf.Pi / 2);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "quaternion", q * Quaternion, 0.4f);
        tween.Finished += UpdateEntrances;
    }

    public void RotateCounterClockwise()
    {
        Quaternion q = new Quaternion(Vector3.Up, Mathf.Pi / 2);
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "quaternion", q * Quaternion, 0.4f).SetTrans(Tween.TransitionType.Back);
        tween.Finished += UpdateEntrances;
    }

    public void CreatePaths(MeshInstance3D Entrance)
    {
        UpdateAdjacencyList();
        UpdateEntrances();

        MeshInstance3D ExitTile = null;
        if (ExitDirection == Direction.North)
        {
            ExitTile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0, 1, -3));
        }
        if (ExitDirection == Direction.South)
        {
            ExitTile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0, 1, 3));
        }
        if (ExitDirection == Direction.East)
        {
            ExitTile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(3, 1, 0));
        }
        if (ExitDirection == Direction.West)
        {
            ExitTile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(-3, 1, 0));
        }
        if (ExitDirection == Direction.Down)
        {
            ExitTile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0, 1, 0));
        }



        Array<Array<MeshInstance3D>> allPaths = FindAllPaths(AdjacencyList, Entrance, ExitTile);
        Array<Path3D> EntrancePaths = new();

        foreach (Array<MeshInstance3D> path in allPaths)
        {
            Path3D temp = new Path3D();
            temp.Curve = new Curve3D();
            AllLanePaths.Add(temp);
            EntrancePaths.Add(temp);

            foreach (MeshInstance3D mesh in path)
            {
                temp.Curve.AddPoint(new Vector3(mesh.Position.X, 0.45f, mesh.Position.Z));
            }

            //don't ask
            temp.Curve.AddPoint(new Vector3(ExitTile.Position.Normalized().X * 4, 0.45f, ExitTile.Position.Normalized().Z * 4));

            //for each entrance, EntrancePathList[entrance] = array of paths

            

            if (Engine.IsEditorHint())
            {
                GetTree().EditedSceneRoot.GetNode(GetPath()).AddChild(temp);
                temp.Owner = GetTree().EditedSceneRoot;

            }
            else
            {
                AddChild(temp);
            }
        }
        EntrancePathList[Entrance] = EntrancePaths;
    }

    public Array<Path3D> GetPathsFromEntrance(MeshInstance3D entrance)
    {
        if (EntrancePathList.ContainsKey(entrance))
        {
            return EntrancePathList[entrance];
        }

        return null;

    }

    public void CheckValidPlacement()
    {
        //check adjacent chunks for compatible entrances
        //cannot connect to more than 1 external entrance

        //check other entrances for gaps
        //if adjacent empty space has entrances leading out of it, not valid
        bool valid = true;

        int ConnectedEntranceCount = 0;

        if (NorthEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.North, NorthTile.GlobalPosition);
            if (tile != null)
            {
                ConnectedEntranceCount++;
            } else
            {

                Vector3 EmptyOrigin = new Vector3(0, 0, -6);
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0,0,-4)).GetMeta("height").AsInt32() == 0) {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0)).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0)).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
            
            }
        }

        if (SouthEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.South, SouthTile.GlobalPosition);
            if (tile != null)
            {
                ConnectedEntranceCount++;
            }
        }

        if (EastEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.East, EastTile.GlobalPosition);
            if (tile != null)
            {
                ConnectedEntranceCount++;
            }
        }

        if (WestEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.West, WestTile.GlobalPosition);
            if (tile != null)
            {
                ConnectedEntranceCount++;
            }
        }

        if (ConnectedEntranceCount > 1)
        {
            valid = false;
            return;
        }





    }


    public override void _Ready()
    {
        UpdateEntrances();
        UpdateAdjacencyList();
        if (!Engine.IsEditorHint())
        {
            PlaceChunk(ExitDirection);
        }
    }

    public override void _Process(double delta)
    {

        base._Process(delta);

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
            PlaceChunk(ExitDirection);
        }

        if (QueryValid)
        {
            QueryValid = false;
            StandardMaterial3D ValidMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryValid.tres");
            StandardMaterial3D ValidLaneMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryValidLane.tres");
            foreach (Node3D node in GetChildren())
            {
                MeshInstance3D mi = node as MeshInstance3D;
                if (mi != null)
                {
                    if (mi.GetMeta("height").AsInt32() == 0) {
                        mi.SetSurfaceOverrideMaterial(0, ValidLaneMaterial);
                    } else
                    {
                        mi.SetSurfaceOverrideMaterial(0, ValidMaterial);
                    }
                }
            }
        }

        if (QueryInvalid)
        {
            QueryInvalid = false;
            StandardMaterial3D InvalidMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryInvalid.tres");
            StandardMaterial3D InvalidLaneMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryInvalidLane.tres");
            foreach (Node3D node in GetChildren())
            {
                MeshInstance3D mi = node as MeshInstance3D;
                if (mi != null)
                {
                    if (mi.GetMeta("height").AsInt32() == 0)
                    {
                        mi.SetSurfaceOverrideMaterial(0, InvalidLaneMaterial);
                    }
                    else
                    {
                        mi.SetSurfaceOverrideMaterial(0, InvalidMaterial);
                    }
                }
            }
        }

        if (ClearOverrides)
        {
            ClearOverrides = false;

            foreach (Node3D node in GetChildren())
            {
                MeshInstance3D mi = node as MeshInstance3D;
                if (mi != null)
                {
                    if (mi.GetMeta("height").AsInt32() == 0)
                    {
                        mi.SetSurfaceOverrideMaterial(0, null);
                    }
                    else
                    {
                        mi.SetSurfaceOverrideMaterial(0, null);
                    }
                }
            }
        }
    }
}
