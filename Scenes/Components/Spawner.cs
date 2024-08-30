using Godot;
using Godot.Collections;
using Managers;
using System;

[Tool]
public partial class Spawner : Path3D
{

    [Export]
    public bool Enabled = true;
    public int ChunkDistance = 0;

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

        if (GetParent<Chunk>().CurrentlyPlacing)
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
        if (GetParent<Chunk>().Disabled)
        {
            Enabled = false;
            return;
        }

        if (!Engine.IsEditorHint())
        {
            WaveManager.GetInstance().RegisterSpawner(this);
        }
        
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

        CheckValid();

        if (GetParent<Chunk>().Disabled || GetParent<Chunk>().CurrentlyPlacing)
        {
            Enabled = false;
            return;
        }

        if (Engine.IsEditorHint())
        {
            return;
        }

        if(Input.IsActionJustPressed("spawn"))
        {
            if (Enabled)
            {
                BaseEnemy b = GD.Load<PackedScene>("res://Scenes/Enemies/Necromancer.tscn").Instantiate<BaseEnemy>();
                AddChild(b);
            }
        }

        ChunkDistance = GetParent<Chunk>().ChunkDistance;
	}
}
