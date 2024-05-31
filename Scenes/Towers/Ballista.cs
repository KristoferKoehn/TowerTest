using Godot;
using System;




[Tool]
public partial class Ballista : Node3D
{

    [Signal]
    public delegate void TowerFiredEventHandler(Node tower, Node target = null);
    [Signal]
    public delegate void TowerPlacedEventHandler(Node tower, Vector3 pos, Node tile);
    [Signal]
    public delegate void TowerSoldEventHandler(Node tower);

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
		SelectOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
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
		Outline.Visible = true;
    }

	public void _on_static_body_3d_mouse_exited()
	{
        Outline.Visible = false;
    }


    public void _on_static_body_3d_input_event(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shape_idx) 
	{

	}
}
