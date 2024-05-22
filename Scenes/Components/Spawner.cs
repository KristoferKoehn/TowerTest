using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class Spawner : Node3D
{

    [Export]
    public bool Enabled = true;


    public void CheckValid()
    {


        MeshInstance3D OriginMesh = RaycastAt(Position + new Vector3(0, 1, 0), Position + new Vector3(0, -1, 0));

        MeshInstance3D NextMesh = RaycastAt(Position + new Vector3(0, 1, -1), Position + new Vector3(0, -1, -1));

        if (OriginMesh != null || NextMesh == null)
        {
            Enabled = false;
            return;
        }

        if (NextMesh.GetMeta("height").AsInt32() != 0 )
        {
            Enabled = false;
            return;
        }

        Enabled = true;

    }



    public MeshInstance3D RaycastAt(Vector3 to, Vector3 from)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(to, from, collisionMask: 8);
        Dictionary result = spaceState.IntersectRay(query);

        if (result.Count > 1)
        {
            return ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
        } else
        {
            return null;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        CheckValid();
	}
}
