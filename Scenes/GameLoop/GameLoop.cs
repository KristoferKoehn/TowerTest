using Godot;
using Godot.Collections;
using System;

public partial class GameLoop : Node3D
{
    Camera3D Camera { get; set; }
    Node3D CameraGimbal { get; set; }
    MeshInstance3D SelectHighlight { get; set; }


    bool turning = false;

    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("CameraGimbal/Camera3D");
        CameraGimbal = GetNode<Node3D>("CameraGimbal");
        SelectHighlight = GetNode<MeshInstance3D>("SelectHighlight");
    }


    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("ui_left"))
        {
            CameraGimbal.TranslateObjectLocal(-Transform.Basis.X / 2.0f);
        }
        if (Input.IsActionPressed("ui_right"))
        {
            CameraGimbal.TranslateObjectLocal(Transform.Basis.X / 2.0f);
        }
        if (Input.IsActionPressed("ui_up"))
        {
            CameraGimbal.TranslateObjectLocal(-Transform.Basis.Z / 2.0f);
        }
        if (Input.IsActionPressed("ui_down"))
        {
            CameraGimbal.TranslateObjectLocal(Transform.Basis.Z / 2.0f);
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

        //raycast at all times to where mouse is

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        Vector3 from = Camera.ProjectRayOrigin(GetViewport().GetMousePosition());
        Vector3 to = from + Camera.ProjectRayNormal(GetViewport().GetMousePosition()) * 1000;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
        Dictionary result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
            SelectHighlight.GlobalPosition = new Vector3(temp.GlobalPosition.X, result["position"].AsVector3().Y + 0.05f, temp.GlobalPosition.Z);
        } else
        {
            SelectHighlight.GlobalPosition = new Vector3(100, 200, 100);
        }


    }

    public override void _Input(InputEvent @event)
    {
        
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            Vector3 from = Camera.ProjectRayOrigin(eventMouseButton.Position);
            Vector3 to = from + Camera.ProjectRayNormal(eventMouseButton.Position) * 1000;

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;


            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 1);
            Dictionary result = spaceState.IntersectRay(query);
            GD.Print(result["position"]);


            query = PhysicsRayQueryParameters3D.Create(from, to, collisionMask: 8);
            result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                MeshInstance3D temp = ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
                GD.Print(temp.Name + " clicked at position " + result["position"]);
            }
        }
        
    }

    private void SetTurning()
    {
        turning = false;
        GD.Print("turn false");
    }
}
