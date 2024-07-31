using Godot;
using Godot.NativeInterop;
using System;

public partial class TowerPanel : Control
{

	[Export]
	Panel TargetSelectPanel { get; set; }

	public AbstractTower SubjectTower { get; set; }



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (SubjectTower != null)
		{
            if (!SubjectTower.Selected)
            {
                QueueFree();
            }

            GlobalPosition = GetViewport().GetCamera3D().UnprojectPosition(SubjectTower.Position) + new Vector2( 25, -100);
		} 
		else
		{
			QueueFree();
		}
    }

	public void _on_target_priority_pressed()
	{
		TargetSelectPanel.Visible = !TargetSelectPanel.Visible;
	}

	public void _on_delete_button_pressed()
	{
		SubjectTower.QueueFree();
		this.QueueFree();
	}
}
