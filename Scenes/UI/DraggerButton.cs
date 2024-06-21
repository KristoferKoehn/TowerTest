using Godot;
using Godot.Collections;
using System;
using static Godot.HttpRequest;

public partial class DraggerButton : Button
{
	[Export]
	Texture2D ButtonIcon;
	[Export]
	PackedScene SceneDraggable;

	bool IsDragging = false;
	Node3D Draggable;
	Camera3D Cam;
	float RAYCAST_LENGTH = 100;
	private BaseMaterial3D _error_mat = GD.Load<BaseMaterial3D>("res://Assets/Materials/QueryInvalidLane.tres");
	bool IsValidLocation = false;
	Vector3 LastValidLocation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		this.Icon = ButtonIcon;
		Draggable = SceneDraggable.Instantiate<Node3D>();
        this.AddChild(Draggable);
		Draggable.Visible = false;
		Cam = GetViewport().GetCamera3D();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _PhysicsProcess(double delta)
    {
        if (IsDragging)
        {
            var SpaceState = Draggable.GetWorld3D().DirectSpaceState;
            Vector2 MousePos = GetViewport().GetMousePosition();
            Vector3 Origin = Cam.ProjectRayOrigin(MousePos);
            Vector3 End = Origin + Cam.ProjectRayNormal(MousePos) * RAYCAST_LENGTH;
            var Query = PhysicsRayQueryParameters3D.Create(Origin, End);
            Query.CollideWithAreas = true;
            Dictionary RayResult = SpaceState.IntersectRay(Query);
            if (RayResult.Count > 0)
            {
                StaticBody3D objectcollidedwith = (StaticBody3D)RayResult["collider"];

                if (objectcollidedwith.GetParent() is MeshInstance3D tile)
				{
					if (tile.GetMeta("height").AsInt32() != 0) // if it isn't a path tile
					{
						Draggable.Visible = true;
						IsValidLocation = true;
						LastValidLocation = new Vector3(tile.GlobalPosition.X, 0.2f, tile.GlobalPosition.Z);
						Draggable.GlobalPosition = LastValidLocation;
						ClearChildMeshError(Draggable);
					}
					else
					{
						Draggable.Visible = true;
						Draggable.GlobalPosition = new Vector3(tile.GlobalPosition.X, 0.2f, tile.GlobalPosition.Z);
						IsValidLocation = false;
						SetChildMeshError(Draggable);
					}
				}
            }
			else
			{
				Draggable.Visible = false;
			}
        }
    }

	public void SetChildMeshError(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is MeshInstance3D mesh)
			{
				SetMeshError(mesh);
			}

			if (child.GetChildCount() > 0)
			{
				SetChildMeshError(child);
			}
		}
	}
    public void ClearChildMeshError(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                ClearMeshError(mesh);
            }

            if (child.GetChildCount() > 0)
            {
                ClearChildMeshError(child);
            }
        }
    }

    public void SetMeshError(MeshInstance3D mesh)
	{
		for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
		{
            mesh.SetSurfaceOverrideMaterial(i, _error_mat);
        }
    }

	public void ClearMeshError(MeshInstance3D mesh)
	{
        for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
        {
            mesh.SetSurfaceOverrideMaterial(i, null);
        }
    }

    public void _on_button_down()
	{
		GD.Print("Dragging");
		IsDragging = true;
		Draggable.Visible = true;
	}

	public void _on_button_up()
	{
		GD.Print("Stopped Dragging");
		IsDragging = false;
		Draggable.Visible = false;
		if (IsValidLocation)
		{
			Node3D Scene = SceneDraggable.Instantiate<Node3D>();
			GetTree().Root.AddChild(Scene);
			Scene.GlobalPosition = LastValidLocation;
		}
	}
}
