using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ballista : Node3D
{

    [Signal]
    public delegate void TowerFiredEventHandler(Node3D tower, Node3D target = null);
    [Signal]
    public delegate void TowerPlacedEventHandler(Node3D tower, Vector3 pos, Node3D tile);
    [Signal]
    public delegate void TowerSoldEventHandler(Node3D tower);

    public StatBlock StatBlock = new();

    public Timer ShotTimer;
    MeshInstance3D TowerBase;
    MeshInstance3D BallistaMount;
    MeshInstance3D BallistaBow;
    MeshInstance3D Arrow;
    MeshInstance3D Outline;

    StandardMaterial3D MouseOverOutline;
    StandardMaterial3D SelectOutline;

    CollisionShape3D RangeHitbox;

    public bool Selected = false;
    public bool MouseOver = false;
    public bool PressWhileMousedOver = false;
    public bool DeselectRejectFlag = false;


    List<BaseEnemy> EnemyList = new List<BaseEnemy>();
    bool CanShoot = false;

    public bool Placing = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BallistaArrowManager.GetInstance().RegisterBallista(this);

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 1.0f},
            {StatType.Damage, 201.0f},
            {StatType.Range, 7.0f},
        };
        StatBlock.SetStatBlock(sb);

        MouseOverOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/MouseOverOutline.tres");
        SelectOutline = GD.Load<StandardMaterial3D>("res://Assets/Materials/SelectOutline.tres");
        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
        BallistaMount = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista");
        BallistaBow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow");
        Arrow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow");
        Outline = GetNode<MeshInstance3D>("Outline/MeshInstance3D");
        RangeHitbox = GetNode<CollisionShape3D>("ActiveRange/CollisionShape3D");
        ShotTimer = GetNode<Timer>("ShotTimer");

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        if (EnemyList.Count > 0)
        {
            int index = EnemyList
            .Select((item, index) => new { Item = item, Index = index, Progress = item.GetProgress() })
            .OrderByDescending(x => x.Progress)
            .First()
            .Index;

            if (index != -1)
            {
                BallistaMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
                    EmitSignal("TowerFired", this, EnemyList[index]);
                    GetNode<AudioStreamPlayer3D>("FiringSound").Play();
                }
            }
        }

        ((CylinderShape3D)RangeHitbox.Shape).Radius = StatBlock.GetStat(StatType.Range);

        if (MouseOver || Selected)
        {
            List<Vector3> points = GeneratePoints(32, GlobalPosition, StatBlock.GetStat(StatType.Range));

            Timer t = new Timer();
            AddChild(t);
            t.Start(0.06);
            t.Timeout += t.QueueFree;

            for (int i = 0; i < points.Count; ++i)
            {
                MeshInstance3D meshInstance3D = new MeshInstance3D();
                ImmediateMesh immediateMesh = new();
                OrmMaterial3D material = new();
                meshInstance3D.Mesh = immediateMesh;
                meshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
                immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
                immediateMesh.SurfaceAddVertex(ToLocal(points[i]));
                if(i == points.Count - 1)
                {
                    immediateMesh.SurfaceAddVertex(ToLocal(points[0]));
                } else
                {
                    immediateMesh.SurfaceAddVertex(ToLocal(points[i + 1]));
                }
                
                immediateMesh.SurfaceEnd();
                material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
                material.AlbedoColor = Colors.WhiteSmoke;
                t.Timeout += meshInstance3D.QueueFree;
                AddChild(meshInstance3D);
            }

        }

	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("deselect"))
        {
            if(DeselectRejectFlag)
            {
                DeselectRejectFlag = false;
            } else
            {
                Selected = false;
                if (MouseOver)
                {
                    Tween t = GetTree().CreateTween();
                    t.TweenProperty(Outline, "scale", new Vector3(1f, 1f, 1f), 0.15).SetTrans(Tween.TransitionType.Back);
                    t.StepFinished += (long idx) => { Outline.Mesh.SurfaceSetMaterial(0, MouseOverOutline); };
                    t.Chain().TweenProperty(Outline, "scale", new Vector3(0.95f, 0.95f, 0.95f), 0.09).SetTrans(Tween.TransitionType.Back);
                    GetNode<AudioStreamPlayer3D>("SelectSound").Play();
                }
                else
                {
                    Tween t = GetTree().CreateTween();
                    t.TweenProperty(Outline, "scale", new Vector3(0.7f, 0.7f, 0.7f), 0.2).SetTrans(Tween.TransitionType.Back);
                }
            }
        }
    }


    public static List<Vector3> GeneratePoints(int n, Vector3 pos, float r)
    {
        List<Vector3> points = new List<Vector3>();
        double angleStep = 2 * Math.PI / n;

        for (int i = 0; i < n; i++)
        {
            double angle = i * angleStep;
            float x = pos.X + r * (float)Math.Cos(angle);
            float z = pos.Z + r * (float)Math.Sin(angle);
            points.Add(new Vector3(x, pos.Y, z));
        }

        return points;
    }

    public void DealDamage(Area3D area)
    {
        BaseEnemy be = area.GetParent<BaseEnemy>();
        be.TakeDamage(StatBlock.GetStat(StatType.Damage), this);
        be.StrikeSound.Play();
        
    }

	public void _on_active_range_area_entered(Area3D area)
	{

		BaseEnemy be = area.GetParent() as BaseEnemy;
		if (be != null)
		{
            EnemyList.Add(be);
        }

    }

	public void _on_active_range_area_exited(Area3D area)
	{
        BaseEnemy be = area.GetParent() as BaseEnemy;
        if (be != null)
        {
            EnemyList.Remove(be);
        }
    }

	public void _on_shot_timer_timeout()
	{
        CanShoot = true;
    }

    public void _on_static_body_3d_mouse_entered()
    {
        if (!Selected)
        {
            Outline.Mesh.SurfaceSetMaterial(0, MouseOverOutline);
        }
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Outline, "scale", new Vector3(0.95f, 0.95f, 0.95f), 0.15).SetTrans(Tween.TransitionType.Back);
        MouseOver = true;
        GetNode<AudioStreamPlayer3D>("SelectSound").Play();
    }

    public void _on_static_body_3d_mouse_exited()
    {
        MouseOver = false;
        PressWhileMousedOver = false;
        if (!Selected)
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(Outline, "scale", new Vector3(0.7f, 0.7f, 0.7f), 0.2).SetTrans(Tween.TransitionType.Back);
        } else
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(Outline, "scale", new Vector3(0.9f, 0.9f, 0.9f), 0.15).SetTrans(Tween.TransitionType.Back);
        }
        
    }

    public void _on_static_body_3d_input_event(Node camera, InputEvent inputEvent, Vector3 position, Vector3 normal, int shape_idx)
    {
        if (inputEvent.IsActionPressed("select"))
        {

            Tween t = GetTree().CreateTween();
            t.TweenProperty(Outline, "scale", new Vector3(0.9f, 0.9f, 0.9f), 0.08).SetTrans(Tween.TransitionType.Back);
            PressWhileMousedOver = true;


            if(!Selected)
            {
                GetNode<AudioStreamPlayer3D>("SelectSound").Play();
            }
            
        }

        if (inputEvent.IsActionReleased("select"))
        {
            InputEventAction deselect = new InputEventAction();
            deselect.Action = "deselect";
            deselect.Pressed = true;
            Input.ParseInputEvent(deselect);

            DeselectRejectFlag = true;
            GetTree().CreateTimer(0.03).Timeout += () => { DeselectRejectFlag = false; };


            if (PressWhileMousedOver)
            {
                Input.ActionPress("deselect");
                Outline.Mesh.SurfaceSetMaterial(0, SelectOutline);
                Selected = true;
                Tween t = GetTree().CreateTween();
                t.TweenProperty(Outline, "scale", new Vector3(0.95f, 0.95f, 0.95f), 0.08).SetTrans(Tween.TransitionType.Back);
            }
            GetNode<AudioStreamPlayer3D>("SelectSound").Play();
        }
    }
}
