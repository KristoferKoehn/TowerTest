using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Shotgun : AbstractTower
{
    PackedScene BulletScene = GD.Load<PackedScene>("res://Resources/TowerProjectiles/MinigunBullet.tscn");
    public List<MeshInstance3D> Bullets = new();

    [Export] MeshInstance3D TowerBase;
    [Export] Node3D ShotgunMount;
    [Export] MeshInstance3D LoadedBullet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.DamageType = DamageType.Physical;
        this.TowerType = TowerType.Shotgun;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 4.0f},
            {StatType.Damage, 200.0f},
            {StatType.Range, 12.0f},
            {StatType.CritRate, 20.0f },
        };
        StatBlock.SetStatBlock(sb);
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
                ShotgunMount.LookAt(EnemyList[index].GlobalPosition);
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
        int bulletCount = 10; // Number of bullets
        float bulletSpread = 1f; // Spread in degrees (you can adjust this value)

        for (int i = 0; i < bulletCount; i++)
        {
            MeshInstance3D bullet = BulletScene.Instantiate<MeshInstance3D>();
            bullet.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) =>
            {
                // Prevent multiple damage events
                if (!bullet.HasMeta("struck"))
                {
                    bullet.SetMeta("struck", "");
                    bullet.GetNode<Area3D>("Hitbox").AreaEntered -= ((Shotgun)tower).DealDamage;
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
            t.Timeout += () =>
            {
                if (bullet != null && !bullet.IsQueuedForDeletion())
                {
                    Bullets.Remove(bullet);
                    bullet.QueueFree();
                }
                t.QueueFree();
            };

            bullet.GetNode<Area3D>("Hitbox").AreaEntered += ((Shotgun)tower).DealDamage;

            bullet.GlobalPosition = LoadedBullet.GlobalPosition;

            // Calculate a random spread direction by slightly offsetting the target's position
            Vector3 randomSpread = GenerateRandomSpread(target.GlobalPosition, bulletSpread);

            // Make the bullet look at the randomly adjusted target position
            bullet.LookAt(randomSpread);

            Bullets.Add(bullet);
            bullet.SetMeta("target", target.GetPath());
        }
    }

    // This function generates a random position around the target based on the spread
    private Vector3 GenerateRandomSpread(Vector3 targetPosition, float spread)
    {
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();

        // Generate random offsets for the x and y (horizontal and vertical spread)
        float randomX = rng.RandfRange(-spread, spread);
        float randomY = rng.RandfRange(-spread, spread);

        // Adjust the target position by the random offsets
        Vector3 spreadPosition = targetPosition + new Vector3(randomX, randomY, 0); // Add spread only in the X and Y axes
        return spreadPosition;
    }


}

