using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Minigun : AbstractTower
{
    PackedScene BulletScene = GD.Load<PackedScene>("res://Resources/TowerProjectiles/MinigunBullet.tscn");
    public List<MeshInstance3D> Bullets = new();

    MeshInstance3D TowerBase;
    Node3D MinigunMount;
    MeshInstance3D LoadedBullet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.DamageType = DamageType.Physical;
        this.TowerType = TowerType.Minigun;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 0.05f},
            {StatType.Damage, 12.0f},
            {StatType.Range, 14.0f},
            {StatType.CritRate, 5.0f },
        };
        StatBlock.SetStatBlock(sb);

        TowerBase = GetNode<MeshInstance3D>("towerRound_base2/tmpParent/towerRound_base");
        MinigunMount = GetNode<Node3D>("MiniGun");
        LoadedBullet = GetNode<MeshInstance3D>("MiniGun/LoadedBullet");
        RangeHitbox = ActiveRange.GetNode<CollisionShape3D>("CollisionShape3D");
    }

    private void MoveBullets(double delta)
    {
        foreach (MeshInstance3D bullet in Bullets)
        {
            bullet.TranslateObjectLocal(new Vector3(0, 0, -100f * (float)delta));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Disabled) return;
        MoveBullets(delta);

        if (EnemyList.Count > 0)
        {
            int index = EnemyList
            .Select((item, index) => new { Item = item, Index = index, Progress = item.GetTotalProgress() })
            .OrderBy(x => x.Progress)
            .First()
            .Index;

            if (index != -1)
            {
                MinigunMount.LookAt(EnemyList[index].GlobalPosition);
                MinigunMount.RotateY(Mathf.Pi); // Rotate 180 degrees if the model is backward
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed) / this.TimeScale);

                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    ShootBullet(this, EnemyList[index]); // this instead.
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

    public void ShootBullet(Node3D tower, Node3D target)
    {
        //make bullet at tower (in the right spot)
        //
        MeshInstance3D bullet = BulletScene.Instantiate<MeshInstance3D>();
        bullet.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            //overdamage protection, prevents damage being dealt to multiple enemies erroneously 
            if (!bullet.HasMeta("struck"))
            {
                bullet.SetMeta("struck", "");
                bullet.GetNode<Area3D>("Hitbox").AreaEntered -= ((Minigun)tower).DealDamage;
                ParticleSignals.GetInstance().createParticle("CandyParticle", bullet.GlobalPosition, bullet.GlobalRotation);
            }
            Bullets.Remove(bullet);
            bullet.QueueFree();
        };
        SceneSwitcher.root.AddChild(bullet);
        Timer t = new Timer();
        bullet.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => {
            if (bullet != null && !bullet.IsQueuedForDeletion())
            {
                Bullets.Remove(bullet);
                bullet.QueueFree();
            }
            t.QueueFree();
        };
        bullet.GetNode<Area3D>("Hitbox").AreaEntered += ((Minigun)tower).DealDamage;
        bullet.GlobalPosition = LoadedBullet.GlobalPosition;
        //bullet.LookAt(target.GlobalPosition + new Vector3(0, 0.5f, 0));
        bullet.LookAt(CalculateShootAhead(0.4f, (BaseEnemy)target));

        Bullets.Add(bullet);
        bullet.SetMeta("target", target.GetPath());
    }

}

