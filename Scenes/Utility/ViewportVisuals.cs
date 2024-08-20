using Godot;
using System;
using TowerTest.Scenes.Components;

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
	public Node SubjectScene { get; set; }

	public override void _Ready()
	{
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

	public void SetSubjectScene(PackedScene packed)
	{
		if (SubjectScene != null)
		{
			SubjectScene.QueueFree();
		}

		SubjectPackedScene = packed;
        var instance = SubjectPackedScene.Instantiate();

        // Check if the instance is of type AbstractPlaceable
        if (instance is AbstractPlaceable placeable)
        {
            SubjectScene = placeable;
            placeable.DisplayMode();
            AddChild(SubjectScene);
        }
        else if (instance is BaseArtifact artifact)
        {
            // Handle BaseArtifact specific logic here if needed
            SubjectScene = artifact;
            //AddChild(SubjectScene);
        }
        else
        {
            GD.PrintErr("The instantiated scene is neither AbstractPlaceable nor BaseArtifact.");
        }
    }

}
