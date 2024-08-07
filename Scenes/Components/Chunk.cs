using Godot;
using Godot.Collections;
using System;
using TowerTest.Scenes.Components;


public enum Direction
{
    North,
    East,
    South,
    West,
    Down
}

[Tool]
public partial class Chunk : AbstractPlaceable
{

    Dictionary<Direction, Vector3> LocalEntrances = new Dictionary<Direction, Vector3>() {
                { Direction.North, new Vector3(0,0,-3)},
                { Direction.East, new Vector3(3,0,0)},
                { Direction.South, new Vector3(0,0,3)},
                { Direction.West, new Vector3(-3,0,0)},
    };

    Dictionary<Direction, Vector3> ExternalEntrances = new Dictionary<Direction, Vector3>() {
                { Direction.North, new Vector3(0,0,-4)},
                { Direction.East, new Vector3(4,0,0)},
                { Direction.South, new Vector3(0,0,4)},
                { Direction.West, new Vector3(-4,0,0)},
    };

    Dictionary<Direction, Vector3> NorthAdjacentOffsets = new Dictionary<Direction, Vector3>() {
        {Direction.North, new Vector3(0, 0, -11)},
        {Direction.East, new Vector3(4, 0, -7) },
        {Direction.West, new Vector3(-4,0, -7) },
    };

    Dictionary<Direction, Vector3> EastAdjacentOffsets = new Dictionary<Direction, Vector3>() {
        {Direction.North, new Vector3(7, 0, -4) },
        {Direction.East, new Vector3(11, 0, 0) },
        {Direction.South, new Vector3(7, 0, 4) },
    };

    Dictionary<Direction, Vector3> SouthAdjacentOffsets = new Dictionary<Direction, Vector3>() {
        {Direction.West, new Vector3(-4, 0, 7) },
        {Direction.East, new Vector3(4, 0, 7) },
        {Direction.South, new Vector3(11, 0, 0) },
    };

    Dictionary<Direction, Vector3> WestAdjacentOffsets = new Dictionary<Direction, Vector3>() {
        {Direction.North, new Vector3(-7, 0, -4) },
        {Direction.West, new Vector3(-11, 0, 0) },
        {Direction.South, new Vector3(-7, 0, 4) },
    };


    //this is set in _Ready()
    Dictionary<Direction, Dictionary<Direction, Vector3>> AdjacentPositionOffsets = new();


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
    [Export]
    public bool CurrentlyPlacing = false;
    [Export]
    public bool Disabled = false;
    [Export]
    public bool Debug = false;
    [Export]
    public bool StaticPlacement = false;

    //this is so we know how big the chunk is. For later.
    public int ChunkSize = 7;

    //this is for tweening.
    bool ChunkRotating = false;
    public bool PlacementValid = false;


    Dictionary<MeshInstance3D, Array<MeshInstance3D>> AdjacencyList = new();
    Array<MeshInstance3D> AllLaneTiles = new();

    public Array<Path3D> AllLanePaths = new();

    Dictionary<MeshInstance3D, Array<Path3D>> EntrancePathList = new();

    public int ChunkDistance = 0;


    /// <summary>
    /// Raycasts 1m out from origin in direction. if Direction.Down is supplied, it will raycast downward.
    /// </summary>
    /// <param name="direction">direction of raycast from given origin, type Direction</param>
    /// <param name="origin">Vector3 Origin of raycast</param>
    /// <returns></returns>
    public MeshInstance3D GetAdjacentTile(Direction direction, Vector3 origin, uint mask = 8)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = null;
        Vector3 EndPosition = Vector3.Zero;


        if (direction == Direction.North)
        {
            EndPosition = origin + new Vector3(0, 0, -1);
        }
        if (direction == Direction.West)
        {
            EndPosition = origin + new Vector3(-1, 0, 0);
        }
        if (direction == Direction.South)
        {
            EndPosition = origin + new Vector3(0, 0, 1);
        }
        if (direction == Direction.East)
        {
            EndPosition = origin + new Vector3(1, 0, 0);
        }
        if (direction == Direction.Down)
        {
            EndPosition = origin + new Vector3(0, -1, 0);
        }


        query = PhysicsRayQueryParameters3D.Create(origin, EndPosition, collisionMask: mask);

        Dictionary result = spaceState.IntersectRay(query);

        if (Debug)
        {

            MeshInstance3D meshInstance3D = new MeshInstance3D();
            meshInstance3D.TopLevel = true;
            ImmediateMesh immediateMesh = new();
            OrmMaterial3D material = new();
            meshInstance3D.Mesh = immediateMesh;
            meshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
            immediateMesh.SurfaceAddVertex(origin);
            immediateMesh.SurfaceAddVertex(EndPosition);

            immediateMesh.SurfaceEnd();
            material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            

            if (result.Count > 1)
            {
                material.AlbedoColor = Colors.Red;
                AddChild(meshInstance3D);
            } else
            {
                material.AlbedoColor = Colors.LimeGreen;
                AddChild(meshInstance3D);
            }

            Timer t = new Timer();
            meshInstance3D.AddChild(t);
            t.Start(0.3);
            t.Timeout += () =>
            {
                meshInstance3D.QueueFree();
                t.QueueFree();
            };

        }

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
        
        MeshInstance3D tile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0, 1, -3), CurrentlyPlacing ? 2048u : 8u);

        if (tile != null)
        {
            
            if (tile.GetMeta("height").AsInt32() == 0)
            {
                if (ExitDirection == Direction.North)
                {
                    tile.SetMeta("exit", true);
                }
                else
                {
                    tile.RemoveMeta("exit");
                }
                NorthTile = tile;
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
            NorthEntrance = false;
        }


        tile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(3, 1, 0), CurrentlyPlacing ? 2048u : 8u);

        if (tile != null)
        {
            
            if (tile.GetMeta("height").AsInt32() == 0)
            {
                if (ExitDirection == Direction.East)
                {
                    tile.SetMeta("exit", true);
                }
                else
                {
                    tile.RemoveMeta("exit");
                }
                EastTile = tile;
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
            EastEntrance = false;
        }

        tile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(-3, 1, 0), CurrentlyPlacing ? 2048u : 8u);

        if (tile != null)
        {
            
            if (tile.GetMeta("height").AsInt32() == 0)
            {
                if (ExitDirection == Direction.West)
                {
                    tile.SetMeta("exit", true);
                }
                else
                {
                    tile.RemoveMeta("exit");
                }
                WestTile = tile;
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
            WestEntrance = false;
        }


        tile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0, 1, 3), CurrentlyPlacing ? 2048u : 8u);
        if (tile != null)
        {
            
            if (tile.GetMeta("height").AsInt32() == 0)
            {
                if (ExitDirection == Direction.South)
                {
                    tile.SetMeta("exit", true);
                }
                else
                {
                    tile.RemoveMeta("exit");
                }
                SouthTile = tile;
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
            SouthEntrance = false;
        }

        tile = GetAdjacentTile(Direction.Down, GlobalPosition + new Vector3(0 , 1, 0), CurrentlyPlacing ? 2048u : 8u);
        if (tile != null)
        {

            if (tile.GetMeta("height").AsInt32() == 0)
            {
                if (ExitDirection == Direction.Down)
                {
                    tile.SetMeta("exit", true);
                }
                else
                {
                    tile.RemoveMeta("exit");
                }
                CenterTile = tile;
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
            CenterEntrance = false;
        }


        //GD.Print($"NORTH: {NorthEntrance} {NorthTile} SOUTH: {SouthEntrance} {SouthTile} EAST: {EastEntrance} {EastTile} WEST: {WestEntrance} {WestTile} CENTER: {CenterEntrance} {CenterTile}");
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
        foreach (Node mesh in GetChildren())
        {
            MeshInstance3D tile = mesh as MeshInstance3D;
            if (tile != null && tile.HasMeta("height"))
            {
                tile.GetNodeOrNull<StaticBody3D>("StaticBody3D").CollisionLayer = 8;
            }
        }

        CurrentlyPlacing = false;
        ClearOverrides = true;
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

        MeshInstance3D connectorTile = null;
        switch (ExitDirection)
        {
            case Direction.North:
                connectorTile = GetAdjacentTile(ExitDirection, NorthTile.GlobalPosition, 8);
                ChunkDistance = connectorTile.GetParent<Chunk>().ChunkDistance + 1;
                break;
            case Direction.South:
                connectorTile = GetAdjacentTile(ExitDirection, SouthTile.GlobalPosition, 8);
                ChunkDistance = connectorTile.GetParent<Chunk>().ChunkDistance + 1;
                break;
            case Direction.West:
                connectorTile = GetAdjacentTile(ExitDirection, WestTile.GlobalPosition, 8);
                ChunkDistance = connectorTile.GetParent<Chunk>().ChunkDistance + 1;
                break;
            case Direction.East:
                connectorTile = GetAdjacentTile(ExitDirection, EastTile.GlobalPosition, 8);
                ChunkDistance = connectorTile.GetParent<Chunk>().ChunkDistance + 1;
                break;
            default:
                ChunkDistance = 0;
                break;
        }

        EmitSignal("Placed", this, this.GlobalPosition);
    }

    public void RotateClockwise()
    {
        if (!ChunkRotating)
        {
            if (CurrentlyPlacing)
            {
                ChunkRotating = true;
                Quaternion q = new Quaternion(Vector3.Up, -Mathf.Pi / 2);
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(this, "quaternion", q * Quaternion, 0.3f).SetTrans(Tween.TransitionType.Back);
                tween.Finished += UpdateEntrances;
                tween.Finished += () => ChunkRotating = false;
                if (CurrentlyPlacing)
                {
                    tween.Finished += CheckValidPlacement;
                }
            } else
            {
                RotateCW = false;
            }
        }
    }

    public void RotateCounterClockwise()
    {
        if (!ChunkRotating)
        {
            if (CurrentlyPlacing)
            {
                ChunkRotating = true;
                Quaternion q = new Quaternion(Vector3.Up, Mathf.Pi / 2);
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(this, "quaternion", q * Quaternion, 0.3f).SetTrans(Tween.TransitionType.Back);
                tween.Finished += UpdateEntrances;
                tween.Finished += () => ChunkRotating = false;
                if (CurrentlyPlacing)
                {
                    tween.Finished += CheckValidPlacement;
                }
            } else
            {
                RotateCCW = false;
            }
        }
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

        UpdateEntrances();

        //check adjacent chunks for compatible entrances
        //cannot connect to more than 1 external entrance

        //check other entrances for gaps
        //if adjacent empty space has entrances leading out of it, not valid
        bool valid = true;

        int ConnectedEntranceCount = 0;
        if (NorthEntrance)
        {   
            MeshInstance3D tile = GetAdjacentTile(Direction.North, NorthTile.GlobalPosition, mask: 8);
            if (tile != null)
            {
                if (tile.GetMeta("height").AsInt32() > 0 || tile.HasMeta("exit"))
                {
                    valid = false;
                } else
                {
                    ConnectedEntranceCount++;
                    ExitDirection = Direction.North;
                }

            } else
            {
                Vector3 EmptyOrigin = GlobalPosition + new Vector3(0, 1, -7);
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
            }
        }

        if (EastEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.East, EastTile.GlobalPosition, mask: 8);
            if (tile != null)
            {
                if (tile.GetMeta("height").AsInt32() > 0 || tile.HasMeta("exit"))
                {
                    valid = false;
                } 
                else
                {
                    ConnectedEntranceCount++;
                    ExitDirection = Direction.East;
                }
                
            }
            else
            {
                Vector3 EmptyOrigin = GlobalPosition + new Vector3(7, 1, 0);
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
            }
        }

        if (SouthEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.South, SouthTile.GlobalPosition, mask: 8);
            if (tile != null)
            {
                if (tile.GetMeta("height").AsInt32() > 0 || tile.HasMeta("exit"))
                {
                    valid = false;
                }
                else
                {
                    ConnectedEntranceCount++;
                    ExitDirection = Direction.South;
                }
            }
            else
            {
                Vector3 EmptyOrigin = GlobalPosition + new Vector3(0, 1, 7);
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
            }
        }

        if (WestEntrance)
        {
            MeshInstance3D tile = GetAdjacentTile(Direction.West, WestTile.GlobalPosition, mask: 8);
            if (tile != null)
            {
                if (tile.GetMeta("height").AsInt32() > 0 || tile.HasMeta("exit"))
                {
                    valid = false;
                }
                else
                {
                    ConnectedEntranceCount++;
                    ExitDirection = Direction.West;
                }
            }
            else
            {
                Vector3 EmptyOrigin = GlobalPosition + new Vector3(-7, 1, 0);
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, -4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(-4, 0, 0), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
                if (GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8) != null && GetAdjacentTile(Direction.Down, EmptyOrigin + new Vector3(0, 0, 4), mask: 8).GetMeta("height").AsInt32() == 0)
                {
                    valid = false;
                }
            }
        }

        if (!(NorthEntrance || EastEntrance || SouthEntrance || WestEntrance || CenterEntrance))
        {
            //no lane chunk boogaloo

            MeshInstance3D tile = GetAdjacentTile(Direction.North, this.GlobalPosition + new Vector3(0, 0, -3), mask: 8);
            if (tile != null && tile.GetMeta("height").AsInt32() == 0)
            {
                valid = false;
            }

            tile = GetAdjacentTile(Direction.East, this.GlobalPosition + new Vector3(3, 0, 0), mask: 8);
            if (tile != null && tile.GetMeta("height").AsInt32() == 0)
            {
                valid = false;
            }

            tile = GetAdjacentTile(Direction.South, this.GlobalPosition + new Vector3(0, 0, 3), mask: 8);
            if (tile != null && tile.GetMeta("height").AsInt32() == 0)
            {
                valid = false;
            }

            tile = GetAdjacentTile(Direction.West, this.GlobalPosition + new Vector3(-3, 0, 0), mask: 8);
            if (tile != null && tile.GetMeta("height").AsInt32() == 0)
            {
                valid = false;
            }
        } else
        {
            if (ConnectedEntranceCount > 1 || ConnectedEntranceCount == 0)
            {
                valid = false;
            }
        }

        if(valid)
        {
            SetOverridesValid();
        } else
        {
            SetOverridesInvalid();
        }

        PlacementValid = valid;
    }


    public override void _Ready()
    {
        base._Ready();

        AdjacentPositionOffsets = new Dictionary<Direction, Dictionary<Direction, Vector3>>()
        {
            {Direction.North, NorthAdjacentOffsets },
            {Direction.West, WestAdjacentOffsets },
            {Direction.South, SouthAdjacentOffsets },
            {Direction.East, EastAdjacentOffsets },
        };

        if (StaticPlacement && !Engine.IsEditorHint() && !Disabled)
        {
            UpdateEntrances();
            UpdateAdjacencyList();
            PlaceChunk(ExitDirection);
        }
    }

    public void SetOverridesInvalid()
    {
        StandardMaterial3D InvalidMaterial = ChunkManager.GetInstance().InvalidMaterial;
        StandardMaterial3D InvalidLaneMaterial = ChunkManager.GetInstance().InvalidLaneMaterial;
        foreach (Node3D node in GetChildren())
        {
            MeshInstance3D mi = node as MeshInstance3D;
            if (mi != null && mi.HasMeta("height"))
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

    public void SetOverridesValid()
    {
        StandardMaterial3D ValidMaterial = ChunkManager.GetInstance().ValidMaterial;
        StandardMaterial3D ValidLaneMaterial = ChunkManager.GetInstance().ValidLaneMaterial;
        foreach (Node3D node in GetChildren())
        {
            MeshInstance3D mi = node as MeshInstance3D;
            if (mi != null && mi.HasMeta("height"))
            {
                if (mi.GetMeta("height").AsInt32() == 0)
                {
                    mi.SetSurfaceOverrideMaterial(0, ValidLaneMaterial);
                }
                else
                {
                    mi.SetSurfaceOverrideMaterial(0, ValidMaterial);
                }
            }
        }
    }

    public void ClearOverrideVisuals()
    {
        foreach (Node3D node in GetChildren())
        {
            MeshInstance3D mi = node as MeshInstance3D;
            if (mi != null && mi.HasMeta("height"))
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

        ClearOverrides = false;
    }

    bool CurrentlyMoving = false;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Disabled)
        {
            return;
        }


        if (CurrentlyPlacing && !Engine.IsEditorHint())
        {
            CheckValidPlacement();
            if (Input.IsActionJustPressed("select") && PlacementValid && !CurrentlyMoving)
            {
                PlaceChunk(ExitDirection);
            }

            if (!CurrentlyMoving)
            {
                Vector3 from = GetViewport().GetCamera3D().ProjectRayOrigin(GetViewport().GetMousePosition());
                Vector3 to = from + GetViewport().GetCamera3D().ProjectRayNormal(GetViewport().GetMousePosition()) * 1000;
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 1);
                Dictionary result = spaceState.IntersectRay(query);

                PhysicsRayQueryParameters3D ChunkCheckQuery = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
                Dictionary ChunkCheckResult = spaceState.IntersectRay(ChunkCheckQuery);

                if (result.Count > 0 && ChunkCheckResult.Count <= 0)
                {
                    Vector3 pos = (Vector3)result["position"];
                    pos = new Vector3(Mathf.Round(pos.X / 7f) * 7f, 0, Mathf.Round(pos.Z / 7f) * 7f);

                    if (pos != GlobalPosition)
                    {
                        CurrentlyMoving = true;
                        Tween t = GetTree().CreateTween();
                        t.TweenProperty(this, "global_position", pos, 0.08);
                        t.Finished += () => {
                            CurrentlyMoving = false;
                        };
                    }
                }
            }
        }


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
            SetOverridesValid();
        }

        if (QueryInvalid)
        {
            QueryInvalid = false;
            SetOverridesInvalid();
        }

        if (ClearOverrides)
        {
            ClearOverrideVisuals();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("rotate_left"))
        {
            RotateCCW = true;
        }

        if (@event.IsActionPressed("rotate_right"))
        {
            RotateCW = true;
        }

        if (@event.IsActionPressed("cancel"))
        {
            if(CurrentlyPlacing)
            {
                EmitSignal("Cancelled");
                QueueFree();
            }
        }
    }

    public override void DisplayMode()
    {
        Disabled = true;
    }

    public override void ActivatePlacing()
    {
        Disabled = false;
        CurrentlyPlacing = true;
        UpdateAdjacencyList();
        UpdateEntrances();

        foreach (Node mesh in GetChildren())
        {
            MeshInstance3D tile = mesh as MeshInstance3D;
            if (tile != null && tile.HasMeta("height"))
            {
                tile.GetNodeOrNull<StaticBody3D>("StaticBody3D").CollisionLayer = 2048;
            }
        }
    }
}
