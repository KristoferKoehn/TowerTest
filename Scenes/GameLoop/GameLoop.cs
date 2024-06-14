using Godot;
using Godot.Collections;
using Managers;
using System;

public partial class GameLoop : Node3D
{
	public Camera3D Camera { get; set; }
	Node3D CameraGimbal { get; set; }

	public Node3D CurrentDraggedChunk { get; set; }


	bool turning = false;

	bool DraggingCamera = false;


	public override void _EnterTree()
	{
		WaveManager.GetInstance();
		BallistaArrowManager.GetInstance();
	}

	public override void _Ready()
	{
        HealthBarManager.GetInstance();

        Camera = GetNode<Camera3D>("CameraGimbal/Camera3D");
		CameraGimbal = GetNode<Node3D>("CameraGimbal");
	}


	public override void _Process(double delta)
	{
		void SetTurning()
		{
			turning = false;
		}

		//make this cooler

		if (Input.IsActionPressed("ui_left"))
		{
			CameraGimbal.TranslateObjectLocal(new Vector3(-0.1f,0,0));
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
		if (Input.IsActionPressed("rotate_right"))
		{
			if (!turning)
			{
				Quaternion q = new Quaternion(Vector3.Up, Mathf.Pi / 2);
				Tween t = GetTree().CreateTween();
				t.TweenProperty(CameraGimbal, "quaternion", q * CameraGimbal.Quaternion, 0.3f);
				turning = true;
				t.Finished += SetTurning;
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
				t.Finished += SetTurning;
			}
		}
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
				CameraGimbal.TranslateObjectLocal(new Vector3(mouseMotion.ScreenRelative.X, 0, mouseMotion.ScreenRelative.Y) * -0.05f);
            }
        }

    }

	public void _on_begin_wave_button_pressed()
	{
		WaveManager.GetInstance().StartWave();
		GD.Print("Button pressed");
	}

}
