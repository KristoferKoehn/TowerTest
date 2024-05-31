using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Ballista : Node3D
{

    [Signal]
    public delegate void TowerFiredEventHandler(Node tower, Node target = null);
    [Signal]
    public delegate void TowerPlacedEventHandler(Node tower, Vector3 pos, Node tile);
    [Signal]
    public delegate void TowerSoldEventHandler(Node tower);

    public StatBlock StatBlock = new();

    public Timer ShotTimer;
    MeshInstance3D TowerBase;
	MeshInstance3D BallistaMount;
	MeshInstance3D BallistaBow;
    MeshInstance3D Arrow;
	MeshInstance3D Outline;

	StandardMaterial3D MouseOverOutline;
	StandardMaterial3D SelectOutline;

	List<BaseEnemy> EnemyList = new();
    bool CanShoot = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{


        Dictionary<StatType, float> sb = new()
		{
			{StatType.AttackSpeed, 1.0f},
            {StatType.Damage, 10.0f},
        };
        StatBlock.SetStatBlock(sb);

		MouseOverOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
		SelectOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
		BallistaMount = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista");
        BallistaBow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow");
        Arrow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow");
		Outline = GetNode<MeshInstance3D>("Outline/MeshInstance3D");
        ShotTimer = GetNode<Timer>("ShotTimer");


        ShotTimer.Timeout += () => EnemyList.ForEach(item => GD.Print("Progress " + item.GetProgress()));



    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

        if (EnemyList.Count > 0)
        {
            EnemyList.OrderByDescending(item => item.GetProgress());
            BallistaMount.LookAt(EnemyList[0].GlobalPosition);
            if (CanShoot)
            {
                CanShoot = false;
                ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
                EmitSignal("TowerFired", this, EnemyList[0]);
                GD.Print("Shooty shoot shoot");
            }
        }
	}



	public void _on_active_range_area_entered(Area3D area)
	{
		BaseEnemy be = area.GetParent() as BaseEnemy;
		if (be != null)
		{
            EnemyList.Add(be);
        }

        EnemyList.OrderByDescending(item => item.GetProgress());

    }

	public void _on_active_range_area_exited(Area3D area)
	{
        BaseEnemy be = area.GetParent() as BaseEnemy;
        if (be != null)
        {
            EnemyList.Remove(be);
        }
        EnemyList.OrderByDescending(item => item.GetProgress());
    }

	public void _on_shot_timer_timeout()
	{
        CanShoot = true;
        if (EnemyList.Count > 0)
        {
            EnemyList.OrderByDescending(item => item.GetProgress());
        }
    }

    //input
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
