using Godot;
using System;

[Tool]
public partial class Ballista : Node3D
{

	MeshInstance3D TowerBase;
	MeshInstance3D BallistaMount;
	MeshInstance3D BallistaBow;
    MeshInstance3D Arrow;
	MeshInstance3D Outline;

	StandardMaterial3D MouseOverOutline;
	StandardMaterial3D SelectOutline;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		MouseOverOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
		SelectOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/SelectOutline.tres");
        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
		BallistaMount = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista");
        BallistaBow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow");
        Arrow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow");
		Outline = GetNode<MeshInstance3D>("Outline/MeshInstance3D");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public void _on_static_body_3d_mouse_entered()
	{
		Outline.Mesh.SurfaceSetMaterial(0, MouseOverOutline);
        Outline.Scale = new Vector3(0.9f, 0.9f, 0.9f);
        Outline.Visible = true;
		GD.Print("mouse entered");
    }

	public void _on_static_body_3d_mouse_exited()
	{
        Outline.Visible = false;
    }


    public void _on_static_body_3d_input_event(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shape_idx) 
	{
		if(inputEvent.IsAction("select"))
		{
			Outline.Scale = new Vector3(1.0f, 1.0f, 1.0f);
            Outline.Mesh.SurfaceSetMaterial(0, SelectOutline);
        }
	}
}
