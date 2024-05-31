using Godot;
using Godot.Collections;
using System;

public partial class GameLoop : Node3D
{
    Camera3D Camera { get; set; }
    Node3D CameraGimbal { get; set; }


    bool turning = false;

    public override void _Ready()
    {
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
    }
}
