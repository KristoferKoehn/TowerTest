using Godot;
using Godot.Collections;
using Managers;

public partial class ChunkTool : Node3D
{
	[Export] Camera3D Camera { get; set; }
	[Export] Node3D CameraGimbal { get; set; }
    [Export] LineEdit FileName { get; set; }
	[Export] FileSelector FileSelector { get; set; }
    [Export] StandardMaterial3D LaneMaterial { get; set; }
    [Export] StandardMaterial3D GrassMaterial { get; set; }


    PackedScene TileScene = GD.Load<PackedScene>("res://Scenes/Components/Tile.tscn");

    Chunk Chunk { get; set; }

	string CurrentFileName = null;

    bool DraggingCamera = false;
    bool Multiselect = false;

    Array<MeshInstance3D> Tiles = new Array<MeshInstance3D>();

    Vector3 MousePosition3D = Vector3.Zero;


    const int DEFAULT_CHUNK_SIZE = 7;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		AddChild(WaveManager.GetInstance());
        
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Chunk != null)
        {
            foreach(Node node in Chunk.GetChildren())
            {
                MeshInstance3D instance = node as MeshInstance3D;
                if (instance != null)
                {
                    if (Tiles.Contains(instance))
                    {
                        instance.Mesh.SurfaceGetMaterial(0).NextPass = GD.Load<ShaderMaterial>("res://Assets/Materials/TileSelectionShader.tres");
                    } else
                    {
                        instance.Mesh.SurfaceGetMaterial(0).NextPass = null;
                    }
                }
            }
        }
	}

    public override void _Input(InputEvent @event)
    {
        Vector3 from = Camera.ProjectRayOrigin(GetViewport().GetMousePosition());
        Vector3 to = from + Camera.ProjectRayNormal(GetViewport().GetMousePosition()) * 1000;

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        /*
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 1);
        result = spaceState.IntersectRay(query);
        */

        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
        Dictionary result = spaceState.IntersectRay(query);

        if (@event.IsActionPressed("shift"))
        {
            Multiselect = true;
        }

        if (@event.IsActionReleased("shift"))
        {
            Multiselect = false;
        }

        if (@event is InputEventMouseButton mouseEvent)
        {

            if (@event.IsActionPressed("select"))
            {
                //getting result from raycast
                if (result.Count > 0)
                {
                    MousePosition3D = (Vector3)result["position"];
                    MeshInstance3D collider = ((Node3D)result["collider"]).GetParent() as MeshInstance3D;
                    if (collider != null)
                    {
                        if (!Tiles.Contains(collider))
                        {
                            if (Multiselect)
                            {
                                Tiles.Add(collider);
                            }
                            else
                            {
                                Tiles.Clear();
                                Tiles.Add(collider);
                            }
                        } else
                        {
                            Tiles.Remove(collider);
                        }
                    }
                } else
                {
                    //Tiles.Clear();
                }
            }
        }

        if (@event.IsActionPressed("deselect"))
        {
            FileName.ReleaseFocus();
        }

        if (@event.IsActionPressed("ui_up"))
        {
            foreach(MeshInstance3D tile in Tiles)
            {
                SetTileHeight(tile, (int)tile.GetMeta("height") + 1);
            }
        }

        if (@event.IsActionPressed("ui_down"))
        {
            foreach (MeshInstance3D tile in Tiles)
            {
                SetTileHeight(tile, (int)tile.GetMeta("height") - 1);
            }
        }

    }

    public void _on_load_button_pressed()
	{
		FileSelector.Visible = true;
	}

	public void _on_file_selector_load_file(string filename)
	{
		if (Chunk != null)
		{
            Chunk.QueueFree();
        }

		Chunk = GD.Load<PackedScene>($"res://Scenes/Chunks/{filename}").Instantiate<Chunk>();
		AddChild( Chunk );
		CurrentFileName = filename.Replace(".tscn", "");
        FileName.Text = CurrentFileName;
    }

	public void _on_save_button_pressed()
	{
        Tiles.Clear();
        //gearbox strat
        this._Process(0.0016f);


		PackedScene chunkScene = new PackedScene();
		Error result = chunkScene.Pack(Chunk);
		if (result == Error.Ok)
		{
			Error error = ResourceSaver.Save(chunkScene, $"res://Scenes/Chunks/{CurrentFileName}.tscn");
			if (error != Error.Ok)
			{
				GD.PushError($"chunk not able to be saved: {error} ");
			}

        } else {
            GD.PushError($"chunk not able to be packed: {result} ");
        }
	}

    public void SetTileHeight(MeshInstance3D tile, int height)
    {
        Vector3 BoxSize = Vector3.Zero;
        switch (height) {
            case 0:
                BoxSize = new Vector3(1, 0.9f, 1);
                break;
            case 1:
                BoxSize = new Vector3(1, 1, 1);
                break;
            case 2:
                BoxSize = new Vector3(1, 1.2f, 1);
                break;
            case 3:
                BoxSize = new Vector3(1, 1.4f, 1);
                break;
            case 4:
                BoxSize = new Vector3(1, 1.6f, 1);
                break;
            default:
                return;
        }
        

        if (height == 0)
        {
            tile.Mesh.SurfaceSetMaterial(0, (StandardMaterial3D)LaneMaterial.Duplicate());
        } else
        {
            tile.Mesh.SurfaceSetMaterial(0, (StandardMaterial3D)GrassMaterial.Duplicate());
        }

        tile.Mesh.SurfaceGetMaterial(0).NextPass = GD.Load<ShaderMaterial>("res://Assets/Materials/TileSelectionShader.tres");

        tile.Position = new Vector3(tile.Position.X, BoxSize.Y/ 2.0f - 0.5f, tile.Position.Z);
        tile.SetMeta("height", height);
        CollisionShape3D collisionShape3D = tile.GetNode<CollisionShape3D>("StaticBody3D/CollisionShape3D");
        ((BoxMesh)tile.Mesh).Size = BoxSize;
        ((BoxShape3D)collisionShape3D.Shape).Size = BoxSize;

        collisionShape3D.Position = Vector3.Zero;
    }

    public void _on_generate_button_pressed()
    {
        if (Chunk != null)
        {
            Chunk.QueueFree();
        }
        CurrentFileName = "";
        FileName.Text = "";

        Chunk = new Chunk();
        AddChild(Chunk);
        Chunk.GenerateNodeData(TileScene, GD.Load<PackedScene>("res://Scenes/Components/Spawner.tscn"));
        
    }

    public void _on_line_edit_text_changed(string filename)
    {
        CurrentFileName = filename;
    }

    public void _on_line_edit_focus_entered()
    {
        Camera.Set("CameraLock", true);
    }

    public void _on_line_edit_focus_exited()
    {
        Camera.Set("CameraLock", false);
    }
}
