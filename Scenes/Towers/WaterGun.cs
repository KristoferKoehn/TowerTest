using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WaterGun : AbstractTower
{
    PackedScene ProjectileScene = GD.Load<PackedScene>("res://Scenes/TowerProjectiles/WaterShot.tscn");
    PackedScene WetEffectScene = GD.Load<PackedScene>("res://Scenes/StatusEffects/WetEffect.tscn");
    public List<MeshInstance3D> WaterShots = new();

    [Export] MeshInstance3D TowerBase;
    [Export] Node3D WaterGunMount;
    [Export] MeshInstance3D LoadedWaterShot;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.DamageType = DamageType.Physical;
        this.TowerType = TowerType.WaterGun;
        LoadedWaterShot.Visible = false;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 0.6f},
            {StatType.Damage, 20.0f},
            {StatType.Range, 11.0f},
            {StatType.CritRate, 10.0f },
        };
        StatBlock.SetStatBlock(sb);
    }

    private void MoveWaterShots(double delta)
    {
        foreach (MeshInstance3D waterShot in WaterShots)
        {
            waterShot.TranslateObjectLocal(new Vector3(0, 0, -80f * (float)delta));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        MoveWaterShots(delta);

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
                WaterGunMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));

                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    ShootWaterShots(this, EnemyList[index]); // this instead.
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

    public void ShootWaterShots(Node3D tower, Node3D target)
    {
        //make waterShot at tower (in the right spot)
        //
        MeshInstance3D waterShot = ProjectileScene.Instantiate<MeshInstance3D>();
        waterShot.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            //overdamage protection, prevents damage being dealt to multiple enemies erroneously 
            if (!waterShot.HasMeta("struck"))
            {
                waterShot.SetMeta("struck", "");
                waterShot.GetNode<Area3D>("Hitbox").AreaEntered -= ((WaterGun)tower).DealDamage;
                ParticleSignals.GetInstance().createParticle("CandyParticle", waterShot.GlobalPosition, waterShot.GlobalRotation);
            }
            WaterShots.Remove(waterShot);
            waterShot.QueueFree();
        };
        SceneSwitcher.root.AddChild(waterShot);
        Timer t = new Timer();
        waterShot.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => {
            if (waterShot != null && !waterShot.IsQueuedForDeletion())
            {
                WaterShots.Remove(waterShot);
                waterShot.QueueFree();
            }
            t.QueueFree();
        };
        waterShot.GetNode<Area3D>("Hitbox").AreaEntered += ((WaterGun)tower).DealDamage;
        waterShot.GlobalPosition = LoadedWaterShot.GlobalPosition;
        //waterShot.LookAt(target.GlobalPosition + new Vector3(0, 0.5f, 0));
        waterShot.LookAt(target.GlobalPosition);

        WaterShots.Add(waterShot);
        waterShot.SetMeta("target", target.GetPath());
    }

    public override void DealDamage(Area3D area)
    {
        base.DealDamage(area);
        WetEffect wetEffect = WetEffectScene.Instantiate<WetEffect>();
        BaseEnemy be = area.GetParent<BaseEnemy>();
        be.AddStatusEffect(wetEffect);
    }

}