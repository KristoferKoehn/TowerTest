using Godot;
using Godot.Collections;
using System;


public partial class TargetPrediction : PathFollow3D
{

    float DistanceRemaining = 0;
    public bool disabled = false;

    public Chunk GetChunkReferenceFromTile(MeshInstance3D tile)
    {
        return tile.GetParent<Chunk>();
    }

    public void AttachNextPath()
    {
        GlobalPosition = GetParent<Path3D>().Curve.SampleBaked(Progress);
        MeshInstance3D temp = GetTileAt(ToGlobal(new Vector3(0, 1, 0)), ToGlobal(new Vector3(0, -1, 0)));

        if (temp == null)
        {
            disabled = true;
            GD.Print(GlobalPosition);
            GD.Print("Can't find chunk");
            return;
        }

        Chunk chunk = GetChunkReferenceFromTile(temp);
 
        Array<Path3D> paths = chunk.GetPathsFromEntrance(temp);

        if (paths != null)
        {
            Random random = new Random();
            int randomIndex = random.Next(paths.Count);
            GD.Print($"Reparenting to {paths[randomIndex].Name}");
            Reparent(paths[randomIndex]);
            ProgressRatio = 0;
        }
    }

    public MeshInstance3D GetTileAt(Vector3 to, Vector3 from)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(to, from, collisionMask: 8);
        Dictionary result = spaceState.IntersectRay(query);

        MeshInstance3D meshInstance3D = new MeshInstance3D();
        meshInstance3D.TopLevel = true;
        ImmediateMesh immediateMesh = new();
        OrmMaterial3D material = new();
        meshInstance3D.Mesh = immediateMesh;
        meshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
        immediateMesh.SurfaceAddVertex(to);
        immediateMesh.SurfaceAddVertex(from);

        immediateMesh.SurfaceEnd();
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;


        if (result.Count > 1)
        {
            material.AlbedoColor = Colors.Red;
            AddChild(meshInstance3D);
        }
        else
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

        if (result.Count > 1)
        {
            return ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
        }
        else
        {
            GD.Print("Didn't find anything");
            return null;
        }
    }

    //function that takes in a float (seconds), calculates it's distance

    public Vector3 GetPositionIn(float seconds, float speed)
    {
        //calculate distance seconds * speed
        DistanceRemaining = seconds * speed;
        Path3D Parent = GetParent<Path3D>();
        Curve3D curve = Parent.Curve;
        //while DistanceRemaining > 0
        //complete current path, switch paths to next,
        //subtract path distance
        GD.Print(DistanceRemaining + " " + disabled);
        while (DistanceRemaining > 0 && !disabled)
        {
            GD.Print($"Progress {Progress}, curve length {curve.GetBakedLength()}");
            if (DistanceRemaining - (curve.GetBakedLength() - Progress) > 0)
            {
                
                // go to end and switch
                DistanceRemaining -= (curve.GetBakedLength() - Progress);
                ProgressRatio = 1;
                GD.Print("Attaching next path");
                AttachNextPath();
            } else
            {
                Progress = DistanceRemaining;
                DistanceRemaining -= Progress;
                GD.Print($"break at the end {DistanceRemaining}");
                break;
            }
            GD.Print("f");
        }

        return GlobalPosition;
    }

}
