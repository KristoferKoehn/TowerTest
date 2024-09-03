using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BubbleGun : AbstractTower
{
    PackedScene ProjectileScene = GD.Load<PackedScene>("res://Resources/TowerProjectiles/Bubble.tscn");
    public List<MeshInstance3D> Bubbles = new();

    [Export] MeshInstance3D TowerBase;
    [Export] Node3D BubbleGunMount;
    [Export] MeshInstance3D LoadedBubble;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.DamageType = DamageType.Water;
        this.TowerType = TowerType.BubbleGun;
        LoadedBubble.Visible = false;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 0.1f},
            {StatType.Damage, 40.0f},
            {StatType.Range, 9.0f},
            {StatType.CritRate, 10.0f },
        };
        StatBlock.SetStatBlock(sb);
    }

    private void MoveBubbles(double delta)
    {
        foreach (MeshInstance3D bubble in Bubbles)
        {
            bubble.TranslateObjectLocal(new Vector3(0, 0, -60f * (float)delta));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        MoveBubbles(delta);

        base._Process(delta);
        if (Disabled) return;

        if (EnemyList.Count > 0)
        {
            int index = EnemyList
            .Select((item, index) => new { Item = item, Index = index, Progress = item.GetTotalProgress() })
            .OrderBy(x => x.Progress)
            .First()
            .Index;

            if (index != -1)
            {
                BubbleGunMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));

                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    ShootBubbles(this, EnemyList[index]); // this instead.
                    FiringSound.Play();
                }
            }
        }

        ((CylinderShape3D)RangeHitbox.Shape).Radius = StatBlock.GetStat(StatType.Range);

        if (MouseOver || Selected || Placing)
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
                if (i == points.Count - 1)
                {
                    immediateMesh.SurfaceAddVertex(ToLocal(points[0]));
                }
                else
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

    public void ShootBubbles(Node3D tower, Node3D target)
    {
        //make bubble at tower (in the right spot)
        //
        MeshInstance3D bubble = ProjectileScene.Instantiate<MeshInstance3D>();
        MakeBubbleRandomSize(bubble);

        bubble.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            //overdamage protection, prevents damage being dealt to multiple enemies erroneously 
            if (!bubble.HasMeta("struck"))
            {
                bubble.SetMeta("struck", "");
                bubble.GetNode<Area3D>("Hitbox").AreaEntered -= ((BubbleGun)tower).DealDamage;
                ParticleSignals.GetInstance().createParticle("CandyParticle", bubble.GlobalPosition, bubble.GlobalRotation);
            }
            Bubbles.Remove(bubble);
            bubble.QueueFree();
        };
        SceneSwitcher.root.AddChild(bubble);
        Timer t = new Timer();
        bubble.AddChild(t);
        t.Start(2); // bubbles pop in two seconds.
        t.OneShot = true;
        t.Timeout += () => {
            if (bubble != null && !bubble.IsQueuedForDeletion())
            {
                Bubbles.Remove(bubble);
                bubble.QueueFree();
            }
            t.QueueFree();
        };
        bubble.GetNode<Area3D>("Hitbox").AreaEntered += ((BubbleGun)tower).DealDamage;
        bubble.GlobalPosition = LoadedBubble.GlobalPosition;
        //bubble.LookAt(target.GlobalPosition + new Vector3(0, 0.5f, 0));
        bubble.LookAt(target.GlobalPosition);

        Bubbles.Add(bubble);
        bubble.SetMeta("target", target.GetPath());
    }

    public void MakeBubbleRandomSize(MeshInstance3D bubble)
    {
        Random rand = new Random();
        int randScale = rand.Next(1, 4);
        bubble.Scale *= randScale;
    }
}