using Godot;
using System;

[Tool]
public partial class Test2 : Node3D
{
	[Export]
	Sprite3D Billboard;

	[Export]
	Node3D target;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.LookAt(EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().Position);

		if (target != null)
		{
            Vector2 targetScreenPosition = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().UnprojectPosition(target.GlobalPosition);
			Vector2 SpriteScreenPosition = EditorInterface.Singleton.GetEditorViewport3D().GetCamera3D().UnprojectPosition(Billboard.GlobalPosition);

			float angle = -Mathf.Atan2((SpriteScreenPosition - targetScreenPosition).Normalized().Y, (SpriteScreenPosition - targetScreenPosition).Normalized().X);
			Quaternion q = new Quaternion(Vector3.Forward, angle);
			Billboard.Quaternion = q;
		}
	}
}
