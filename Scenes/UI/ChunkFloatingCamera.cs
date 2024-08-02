using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class ChunkFloatingCamera : Node3D
{

    [ExportGroup("Camera Settings")]

    [Export]
    public Node3D CameraGimbal { get; set; }

    [Export]
    public Node3D CameraRotationPoint { get; set; }

    [Export]
    public Camera3D Camera { get; set; }

    [Export]
    public bool OrbitingCamera = true;

    [Export]
    public Viewport viewport { get; set; }

    [Export(PropertyHint.Range, "-4, 4")]
    public float CameraOrbitSpeed = 1.0f;

    [Export(PropertyHint.Range, "-40, 40")]
    public float CameraTilt = 0.0f;

    [Export(PropertyHint.Range, "0, 40")]
    public float CameraZoom = 5.6f;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CameraRotationPoint.GlobalPosition = this.GlobalPosition;
        CameraGimbal.LookAt(CameraRotationPoint.GlobalPosition);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (OrbitingCamera)
        {
            CameraRotationPoint.Rotate(Vector3.Up, CameraOrbitSpeed * (float)delta);
            Camera.Quaternion = new Quaternion(Vector3.Left, Mathf.DegToRad(CameraTilt));
        }

        CameraRotationPoint.GlobalPosition = this.GlobalPosition;
    }
}
