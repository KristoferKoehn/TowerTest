using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using TowerTest.Scenes.Components;

public enum TowerType
{
    Ballista,
    Cannon,
    Catapult,
    GravityCrystal,
    LaserCrystal,
    Minigun,
    BubbleGun,
    WaterGun,
    Flamethrower,
    Shotgun,
}

[Tool]
public abstract partial class AbstractTower : AbstractPlaceable
{
    public bool Debugging = false;
    public DamageType DamageType;
    public TowerType TowerType { get; set; }

    [Signal] public delegate void TowerFiredEventHandler(Node3D tower, Node3D target = null);
    [Signal] public delegate void TowerSoldEventHandler(Node3D tower);

    [Export] public MeshInstance3D Outline;
    [Export] public StandardMaterial3D MouseOverOutline;
    [Export] public StandardMaterial3D SelectOutline;
    [Export] public StandardMaterial3D InvalidMaterial;
    [Export] public StandardMaterial3D ValidMaterial;
    [Export] public StaticBody3D SelectorHitbox;
    [Export] public Area3D ActiveRange;
    [Export] public Timer ShotTimer;
    [Export] public AudioStreamPlayer3D FiringSound;
    [Export] public CollisionShape3D RangeHitbox;

    public PackedScene DummyEnemyScene = GD.Load<PackedScene>("res://Scenes/Enemies/BaseEnemy.tscn");

    public bool Disabled = false;
    public bool Selected = false;
    public bool MouseOver = false;
    public bool PressWhileMousedOver = false;
    public bool DeselectRejectFlag = false;
    public float TimeScale = 1.0f;

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
        if (Placing)
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
        PrevPlacing = Placing;
    }

    Vector3 PlaceSpot = Vector3.Zero;
    MeshInstance3D currentTile = null;
    bool PrevPlacing = false;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        delta *= this.TimeScale;

        if (Disabled) return;

        if (!PrevPlacing && Placing)
        {
            indicator = new MeshInstance3D();
            QuadMesh q = new QuadMesh();
            q.Orientation = PlaneMesh.OrientationEnum.Y;
            indicator.Mesh = q;
            SelectorHitbox.GetNode<CollisionShape3D>("CollisionShape3D").Disabled = true;
            AddChild(indicator);
        }

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
                    EmitSignal("Placed", this, PlaceSpot);


                    //get rid of this one, gameplay necessitates this is not a feature
                    //shift multiplacement
                    /*
                    if (Input.IsActionPressed("shift"))
                    {
                        PackedScene ps = GD.Load<PackedScene>(SceneFilePath);
                        AbstractTower at = ps.Instantiate<AbstractTower>();
                        at.Placing = true;
                        GetParent().AddChild(at);
                        at.GlobalPosition = GlobalPosition;
                    }
                    */

                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed) / this.TimeScale);
                }
            }

            if (Input.IsActionJustPressed("cancel"))
            {
                this.QueueFree();
                EmitSignal("Cancelled");
            }
        }

        PrevPlacing = Placing;
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

    public Vector3 CalculateShootAhead(float distance, BaseEnemy enemy)
    {
        // Create a temporary PathFollow3D node to simulate the progress
        //BaseEnemy tempPathFollow = DummyEnemyScene.Instantiate<BaseEnemy>();

        // Set the initial progress to the enemy's current progress
        //tempPathFollow.Progress = enemy.Progress + distance;

        // Get the global position at this new progress
        //Vector3 shootAheadPos = tempPathFollow.GlobalPosition;

        // No need to add this node to the scene, just return the position
        //return shootAheadPos;
        return enemy.GlobalPosition;
    }

    public virtual void DealDamage(Area3D area)
    {
        if (!(area.GetParent() is BaseEnemy)) return;

        BaseEnemy be = area.GetParent<BaseEnemy>();
        bool isCrit = false;

        // Get the crit rate and clamp it to 100%
        float critRate = this.StatBlock.GetStat(StatType.CritRate);
        critRate = Math.Min(critRate, 100f); // Ensure crit rate does not exceed 100%

        Random rand = new Random();
        int randomNum = rand.Next(0, 100);

        // Determine if the attack is a critical hit
        if (randomNum < critRate)
        {
            isCrit = true;
        }

        // Calculate damage
        float normalDamage = StatBlock.GetStat(StatType.Damage);
        float critMultiplier = 1.5f; // Assuming CritMultiplier is a multiplier (e.g., 1.5x for 50% bonus)
        float damage = normalDamage;

        if (isCrit)
        {
            damage *= critMultiplier; // Apply crit multiplier if it's a crit
        }

        // Apply damage to the enemy
        be.TakeDamage(damage, this, isCrit, this.DamageType);
        be.StrikeSound.Play();
    }

    public void DealDamageAOE()
    {
        EmitSignal("TowerFired", this);
        foreach (BaseEnemy be in EnemyList)
        {
            bool isCrit = false;

            // Get the crit rate and clamp it to 100%
            float critRate = this.StatBlock.GetStat(StatType.CritRate);
            critRate = Math.Min(critRate, 100f); // Ensure crit rate does not exceed 100%

            Random rand = new Random();
            int randomNum = rand.Next(0, 100);

            // Determine if the attack is a critical hit
            if (randomNum < critRate)
            {
                isCrit = true;
            }

            // Calculate damage
            float normalDamage = StatBlock.GetStat(StatType.Damage);
            float critMultiplier = 1.5f; // Assuming CritMultiplier is a multiplier (e.g., 1.5x for 50% bonus)
            float damage = normalDamage;

            if (isCrit)
            {
                damage *= critMultiplier; // Apply crit multiplier if it's a crit
            }
            be.TakeDamage(damage, this, isCrit, this.DamageType);
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
            points.Add(new Vector3(x, pos.Y + 0.4f, z));
        }

        return points;
    }

    public override void DisplayMode()
    {
        Disabled = true;
    }

    public override void ActivatePlacing()
    {
        GlobalPosition = SceneSwitcher.CurrentGameLoop.MousePosition3D;
        Disabled = false;
        Placing = true;
        TowerManager.GetInstance().RegisterTower(this);
    }
}
