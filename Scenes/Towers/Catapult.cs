using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class Catapult : AbstractTower
{
    PackedScene CatapultBallScene = GD.Load<PackedScene>("res://Resources/TowerProjectiles/CatapultBall.tscn");
    public List<(MeshInstance3D ball, Vector3 velocity)> CatapultBalls = new();

    [Export] public AnimationPlayer _animationPlayer;
    public MeshInstance3D TowerBase;
    public MeshInstance3D CatapultMount;
    public MeshInstance3D LoadedCatapultBall;
    private Vector3 LastFrameEnemyVelocity;

    public float SplashRadius { get; set; } = 1; // 1 m

    public override void _Ready()
    {
        this.DamageType = DamageType.Physical;
        this.TowerType = TowerType.Catapult;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        System.Collections.Generic.Dictionary<StatType, float> sb = new()
        {
            { StatType.AttackSpeed, 4.0f },
            { StatType.Damage, 1000.0f },
            { StatType.Range, 16.0f },
            {StatType.CritRate, 10.0f },
        };
        StatBlock.SetStatBlock(sb);

        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
        CatapultMount = GetNode<MeshInstance3D>("weapon_catapult2/tmpParent/weapon_catapult");
        LoadedCatapultBall = GetNode<MeshInstance3D>("weapon_catapult2/tmpParent/weapon_catapult/catapult/CannonBall");
        LoadedCatapultBall.Visible = false;
    }

    public override void _Process(double delta)
    {
        MoveCatapultBalls(delta);
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
                CatapultMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
                    LaunchCatapult(this, EnemyList[index]);
                    GetNode<AudioStreamPlayer3D>("FiringSound").Play();
                }
            }
        }

        ((CylinderShape3D)RangeHitbox.Shape).Radius = StatBlock.GetStat(StatType.Range);

        // Debugging visuals for range
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

    private void MoveCatapultBalls(double delta)
    {
        double ballSpeed = 5 * delta;
        float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        for (int i = CatapultBalls.Count - 1; i >= 0; i--)
        {
            var (catapultBall, velocity) = CatapultBalls[i];

            // Apply gravity to the vertical velocity
            velocity.Y -= gravity * (float)ballSpeed;

            // Move the ball based on the updated velocity
            catapultBall.Translate(velocity * (float)ballSpeed);

            // If the ball is below a certain height, assume it hit the ground and remove it
            if (catapultBall.GlobalPosition.Y < 0)
            {
                CatapultBalls.RemoveAt(i);
                catapultBall.QueueFree();
            }
            else
            {
                // Update the velocity in the list
                CatapultBalls[i] = (catapultBall, velocity);
            }
        }
    }


    public void LaunchCatapult(Node3D tower, Node3D target)
    {
        Catapult catapult = tower as Catapult;
        catapult._animationPlayer.Play("launch_catapult");

        MeshInstance3D catapultBall = CatapultBallScene.Instantiate<MeshInstance3D>();
        catapultBall.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            if (!catapultBall.HasMeta("struck"))
            {
                //catapultBall.SetMeta("struck", "");

                //catapultBall.GetNode<Area3D>("Hitbox").AreaEntered -= ((Catapult)tower).DealDamage;

                //ParticleSignals.GetInstance().createParticle("CandyParticle", catapultBall.GlobalPosition, catapultBall.GlobalRotation);
            }
            //CatapultBalls.Remove(CatapultBalls.First(x => x.ball == catapultBall));
            //catapultBall.QueueFree();
        };

        SceneSwitcher.root.AddChild(catapultBall);
        Timer t = new Timer();
        catapultBall.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => {
            if (catapultBall != null && !catapultBall.IsQueuedForDeletion())
            {
                CatapultBalls.Remove(CatapultBalls.First(x => x.ball == catapultBall));
                catapultBall.QueueFree();
            }
            t.QueueFree();
        };

        catapultBall.GetNode<Area3D>("Hitbox").AreaEntered += ((Catapult)tower).DealDamage;
        catapultBall.GlobalPosition = LoadedCatapultBall.GlobalPosition;

        // Launch stuff:
        Vector3 distanceToTarget = target.GlobalPosition - catapultBall.GlobalPosition;
        float horizontalDistance = new Vector2(distanceToTarget.X, distanceToTarget.Z).Length();
        float heightDifference = distanceToTarget.Y;
        float launchAngle = Mathf.DegToRad(45); // Adjust this angle if needed
        float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

        float speed = Mathf.Sqrt((gravity * Mathf.Pow(horizontalDistance, 2)) /
                    (horizontalDistance * Mathf.Sin(2 * launchAngle) - 2 * heightDifference * Mathf.Pow(Mathf.Cos(launchAngle), 2)));

        Vector3 initialVelocity = new Vector3(
            distanceToTarget.X / horizontalDistance * speed * Mathf.Cos(launchAngle),
            speed * Mathf.Sin(launchAngle),
            (distanceToTarget.Z / horizontalDistance * speed * Mathf.Cos(launchAngle))
        );

        // Add the calculated initial velocity to the list
        CatapultBalls.Add((catapultBall, initialVelocity));
    }
}
