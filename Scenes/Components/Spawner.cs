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

        MeshInstance3D OriginMesh = RaycastAt(ToGlobal(new Vector3(0, 1, 0)), ToGlobal(new Vector3(0, -1, 0)));

        MeshInstance3D NextMesh = RaycastAt(ToGlobal(new Vector3(0, 1, -1)), ToGlobal(new Vector3(0, -1, -1)));


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



    public MeshInstance3D RaycastAt(Vector3 from, Vector3 to)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
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
        CheckValid();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        
        if(Engine.IsEditorHint())
        {
            return;
        }

        if(Input.IsActionJustPressed("spawn"))
        {
            if (Enabled)
            {
                BaseEnemy b = GD.Load<PackedScene>("res://Scenes/Enemies/BaseEnemy.tscn").Instantiate<BaseEnemy>();
                GetNode<Path3D>("Path3D").AddChild(b);
            }
        }
	}
}
