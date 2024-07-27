using Godot;
using System;

[Tool]
public partial class ViewportVisuals : SubViewport
{
	[ExportGroup("Camera Settings")]
	[Export]
	public Vector3 InitialCameraPosition { get; set; } = (Vector3.Back + Vector3.Up + Vector3.Right).Normalized();

    [Export]
	public Node3D CameraGimbal { get; set; }

    [Export]
    public Node3D CameraRotationPoint { get; set; }

    [Export]
	public Camera3D Camera { get; set; }

	[Export]
	public bool OrbitingCamera = true;

	[Export(PropertyHint.Range, "-4, 4")]
	public float CameraOrbitSpeed = 1.0f;

	[Export(PropertyHint.Range, "-40, 40")]
	public float CameraTilt = 0.0f;

    [Export(PropertyHint.Range, "0, 40")]
    public float CameraZoom = 5.6f;


    [ExportGroup("")]
    [Export]
	public PackedScene SubjectPackedScene { get; set; }
	public Node3D SubjectScene { get; set; }

	public override void _Ready()
	{
		SubjectScene = SubjectPackedScene.Instantiate<Node3D>();
		Chunk c = SubjectScene as Chunk;
		if (c != null)
		{
			c.Disabled = true;
		}

		BaseEnemy be = SubjectScene as BaseEnemy;
		if (be != null)
		{
			be.Disabled = true;
		}


		AddChild(SubjectScene);
		CameraGimbal.Position = InitialCameraPosition.Normalized() * CameraZoom;
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

		CameraGimbal.Position = InitialCameraPosition.Normalized() * CameraZoom;
	}
}
