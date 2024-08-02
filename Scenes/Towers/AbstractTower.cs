using Godot;
using Godot.Collections;
using MMOTest.Backend;
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
    public StandardMaterial3D InvalidMaterial;
    [Export]
    public StandardMaterial3D ValidMaterial;
    [Export]
    public StaticBody3D SelectorHitbox;
    [Export]
    public Area3D ActiveRange;
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
    [Export]
    PackedScene TowerPanel;

    public StatBlock StatBlock = new();

    public List<BaseEnemy> EnemyList = new List<BaseEnemy>();


    MeshInstance3D indicator { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        if(Placing)
        {
            indicator = new MeshInstance3D();
            QuadMesh q = new QuadMesh();
            q.Orientation = PlaneMesh.OrientationEnum.Y;
            indicator.Mesh = q;
            SelectorHitbox.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
            AddChild(indicator);
        }

        ActiveRange.AreaEntered += _on_active_range_area_entered;
        ActiveRange.AreaExited += _on_active_range_area_exited;

    }

    Vector3 PlaceSpot = Vector3.Zero;
    MeshInstance3D currentTile = null;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Placing)
        {
            CanShoot = false;
            //get raycast
            var SpaceState = GetWorld3D().DirectSpaceState;
            Vector2 MousePos = GetViewport().GetMousePosition();
            Vector3 Origin = GetViewport().GetCamera3D().ProjectRayOrigin(MousePos);
            Vector3 End = Origin + GetViewport().GetCamera3D().ProjectRayNormal(MousePos) * 1000f;

            var Query = PhysicsRayQueryParameters3D.Create(Origin, End);
            Dictionary RayResult = SpaceState.IntersectRay(Query);

            if (RayResult.Count > 0 && ((Node3D)RayResult["collider"]).GetParent() != this)
            {
                Tween t = GetTree().CreateTween();
                
                Vector3 pos = (Vector3)RayResult["position"];
                pos = new Vector3(Mathf.Round(pos.X), pos.Y, Mathf.Round(pos.Z));
                PlaceSpot = pos;
                t.TweenProperty(this, "global_position", pos + new Vector3(0, 0.6f, 0), 0.1);
                
                indicator.GlobalPosition = pos + new Vector3(0, 0.1f, 0);
                
                MeshInstance3D tile = ((Node3D)RayResult["collider"]).GetParent() as MeshInstance3D;
                if (tile != null && tile.HasMeta("height") && !tile.HasMeta("tile_invalid"))
                {
                    currentTile = tile;
                    if (tile.GetMeta("height").AsInt32() > 0)
                    {
                        Valid = true;
                        SetChildMaterialOverride(this, ValidMaterial);
                    } else
                    {
                        Valid = false;
                        SetChildMaterialOverride(this, InvalidMaterial);
                    }

                } else
                {
                    Valid = false;
                    SetChildMaterialOverride(this, InvalidMaterial);
                    currentTile = null;
                }
            }

            //placement
            if (Input.IsActionJustReleased("select"))
            {
                if (Valid && currentTile != null)
                {
                    SetChildMaterialOverride(this, null);
                    indicator.Visible = false;
                    Tween t = CreateTween();
                    t.TweenProperty(this, "global_position", PlaceSpot, 0.1);
                    Placing = false;
                    SelectorHitbox.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = false;
                    currentTile.SetMeta("tile_invalid", true);

                    //shift multiplacement
                    if (Input.IsActionPressed("shift"))
                    {
                        PackedScene ps = GD.Load<PackedScene>(SceneFilePath);
                        AbstractTower at = ps.Instantiate<AbstractTower>();
                        at.Placing = true;
                        GetParent().AddChild(at);
                        at.GlobalPosition = GlobalPosition;
                    }

                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
                }

            }

            if (Input.IsActionJustPressed("cancel"))
            {
                this.QueueFree();
            }
        }

        EnemyList.RemoveAll(item => item.dead);
    }

    public void SetChildMaterialOverride(Node node, StandardMaterial3D sm3d)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                SetMeshMaterialOverride(mesh, sm3d);
            }

            if (child.GetChildCount() > 0)
            {
                SetChildMaterialOverride(child, sm3d);
            }
        }
    }

    public void SetMeshMaterialOverride(MeshInstance3D mesh, StandardMaterial3D sm3d)
    {
        for (int i = 0; i < mesh.GetSurfaceOverrideMaterialCount(); i++)
        {
            mesh.SetSurfaceOverrideMaterial(i, sm3d);
        }
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
                if (!Selected)
                {
                    TowerPanel tp = TowerPanel.Instantiate<TowerPanel>();
                    tp.SubjectTower = this;
                    GetParent<GameLoop>().GetNode<Control>("UI/Control").AddChild(tp);
                }

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
