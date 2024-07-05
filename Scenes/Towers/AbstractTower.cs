using Godot;
using System;
using System.Collections.Generic;

[Tool]
public abstract partial class AbstractTower : Node3D
{


    [Signal]
    public delegate void TowerFiredEventHandler(Node3D tower, Node3D target = null);
    [Signal]
    public delegate void TowerPlacedEventHandler(Node3D tower, Vector3 pos, Node3D tile);
    [Signal]
    public delegate void TowerSoldEventHandler(Node3D tower);


    [Export]
    public MeshInstance3D Outline;
    [Export]
    public StandardMaterial3D MouseOverOutline;
    [Export]
    public StandardMaterial3D SelectOutline;
    [Export]
    public StaticBody3D SelectorHitbox;
    [Export]
    public Timer ShotTimer;

    public bool Selected = false;
    public bool MouseOver = false;
    public bool PressWhileMousedOver = false;
    public bool DeselectRejectFlag = false;

    public bool CanShoot = false;

    [Export]
    public bool Placing = false;
    [Export]
    public bool Valid = false;

    public List<BaseEnemy> EnemyList = new List<BaseEnemy>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Placing)
        {
            //CheckValid
            if (Input.IsActionJustReleased("select"))
            {
                if (Valid)
                {
                    //place tower
                }
            }
        }
	}

    public void CheckValid()
    {

    }

    //what do all towers/buildings? need to do?
    //shoot
    //mouse over select
    //click select
    //unselect on click anywhere else
    //buy
    //sell
    //place - !!!


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
        }
        else
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


            if (!Selected)
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
                PressWhileMousedOver = false;
                Outline.Mesh.SurfaceSetMaterial(0, SelectOutline);
                Selected = true;
                Tween t = GetTree().CreateTween();
                t.TweenProperty(Outline, "scale", new Vector3(0.95f, 0.95f, 0.95f), 0.08).SetTrans(Tween.TransitionType.Back);
            }
            GetNode<AudioStreamPlayer3D>("SelectSound").Play();


        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("deselect"))
        {

            if (DeselectRejectFlag)
            {
                DeselectRejectFlag = false;
            }
            else
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


    public void _on_shot_timer_timeout()
    {
        CanShoot = true;
    }


}
