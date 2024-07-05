using Godot;
using Godot.Collections;

public partial class DraggerButton : Button
{
	[Export]
	Texture2D ButtonIcon;
	[Export]
	PackedScene SpawnScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		this.Icon = ButtonIcon;
		
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

	// For recursively setting the material of all children nodes (towers) to red
	// if it is an invalid placement.
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

    // For recursively setting the material of all children nodes (towers) back to
	// normal when there is a valid placement.
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

    }

    public void _on_button_up()
	{

	}

    public void _on_pressed()
	{
        AbstractTower tower = SpawnScene.Instantiate<AbstractTower>();
        tower.Placing = true;
        SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(tower);
    }

}
