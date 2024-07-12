using Godot;
using Godot.Collections;
using Managers;
using System;

public partial class GameLoop : Node3D
{
    private StandardMaterial3D _tile_outline_mat = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
    private MeshInstance3D _currently_highlighted_tile; // The currently highlighted tile

    public Camera3D Camera { get; set; }
	Node3D CameraGimbal { get; set; }

    //private AbstractTower selectedTower; // Reference to the currently selected tower

    bool turning = false;

	bool DraggingCamera = false;


	public override void _EnterTree()
	{
		WaveManager.GetInstance();
		BallistaArrowManager.GetInstance();
		ParticleSignals.GetInstance();
		ParticleManager.GetInstance();
	}

	public override void _Ready()
	{
        HealthBarManager.GetInstance();

        Camera = GetNode<Camera3D>("CameraGimbal/Camera3D");
		CameraGimbal = GetNode<Node3D>("CameraGimbal");
	}


	public override void _Process(double delta)
	{

        //make this cooler

        /*
		if (Input.IsActionPressed("rotate_right"))
		{
			if (!turning)
			{
				Quaternion q = new Quaternion(Vector3.Up, Mathf.Pi / 2);
				Tween t = GetTree().CreateTween();
				t.TweenProperty(CameraGimbal, "quaternion", q * CameraGimbal.Quaternion, 0.3f);
				turning = true;
				t.Finished += () => turning = false;
			}
		}
		if (Input.IsActionPressed("rotate_left"))
		{
			if (!turning)
			{
				Quaternion q = new Quaternion(Vector3.Up, -Mathf.Pi / 2);
				Tween t = GetTree().CreateTween();
				t.TweenProperty(CameraGimbal, "quaternion", q * CameraGimbal.Quaternion, 0.3f);
				turning = true;
				t.Finished += () => turning = false;
            }
		}
		*/
        HandleCameraMovement();
		CheckMouseHover();
    }

    private void HandleCameraMovement()
    {
        if (Input.IsActionPressed("ui_left"))
        {
            CameraGimbal.TranslateObjectLocal(new Vector3(-0.1f, 0, 0));
        }
        if (Input.IsActionPressed("ui_right"))
        {
            CameraGimbal.TranslateObjectLocal(new Vector3(0.1f, 0, 0));
        }
        if (Input.IsActionPressed("ui_up"))
        {
            CameraGimbal.TranslateObjectLocal(new Vector3(0, 0, -0.1f));
        }
        if (Input.IsActionPressed("ui_down"))
        {
            CameraGimbal.TranslateObjectLocal(new Vector3(0, 0, 0.1f));
        }
    }

    private void CheckMouseHover()
    {
        Vector2 mousePosition = GetViewport().GetMousePosition();
        Vector3 from = Camera.ProjectRayOrigin(mousePosition);
        Vector3 to = from + Camera.ProjectRayNormal(mousePosition) * 1000;

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to);
        Dictionary result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            // Doing this: MeshInstance3D tile = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            // But with safety

            StaticBody3D temp = (StaticBody3D)result["collider"];
            MeshInstance3D tile = temp.GetParentOrNull<MeshInstance3D>();
            if (tile == null)
            {
                return;
            }

            if (tile != _currently_highlighted_tile)
            {
                if (_currently_highlighted_tile != null)
                {
                    ResetOutline(_currently_highlighted_tile);
                }

                ApplyOutline(tile);
                _currently_highlighted_tile = tile;
            }
        }
        else if (_currently_highlighted_tile != null)
        {
            ResetOutline(_currently_highlighted_tile);
            _currently_highlighted_tile = null;
        }
    }

    private void ApplyOutline(MeshInstance3D meshInstance)
    {
        meshInstance.MaterialOverride = _tile_outline_mat;
    }

    private void ResetOutline(MeshInstance3D meshInstance)
    {
        meshInstance.MaterialOverride = null;
    }

    public override void _Input(InputEvent @event)
	{
		
		if (@event.IsActionPressed("select"))
		{
			Vector3 from = Camera.ProjectRayOrigin(GetViewport().GetMousePosition());
			Vector3 to = from + Camera.ProjectRayNormal(GetViewport().GetMousePosition()) * 1000;

			PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;


			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 1);
			Dictionary result = spaceState.IntersectRay(query);


			query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
			result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            }
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (@event.IsActionPressed("zoom_in"))
			{
                if (Camera.Position.Length() > 7)
                {
					Tween t = GetTree().CreateTween();
					t.TweenProperty(Camera, "position", Camera.Position - new Vector3(0, 2.5f, 2.5f), 0.06);
                }
            }
            if (@event.IsActionPressed("zoom_out"))
            {
                if (Camera.Position.Length() < 50)
                {
                    Tween t = GetTree().CreateTween();
                    t.TweenProperty(Camera, "position", Camera.Position + new Vector3(0, 2.5f, 2.5f), 0.06);
                }
            }

			if (@event.IsActionPressed("camera_drag"))
			{
				DraggingCamera = true;
            } 
			if(@event.IsActionReleased("camera_drag"))
			{
                DraggingCamera = false;
            }
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {

            if (DraggingCamera)
            {
				//CameraGimbal.TranslateObjectLocal(new Vector3(mouseMotion.ScreenRelative.X, 0, mouseMotion.ScreenRelative.Y) * -0.05f);
            }
        }

    }

	public void _on_begin_wave_button_pressed()
	{
		WaveManager.GetInstance().StartWave();
		GD.Print("Button pressed");
	}
}
