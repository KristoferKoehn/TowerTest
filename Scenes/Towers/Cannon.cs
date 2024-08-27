using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Cannon : AbstractTower
{
    PackedScene CannonBallScene = GD.Load<PackedScene>("res://Scenes/Components/CannonBall.tscn");
    public List<MeshInstance3D> CannonBalls = new();

    private bool Debugging = false;

    [Export] public AnimationPlayer _animationPlayer;
    public MeshInstance3D TowerBase;
    public MeshInstance3D CannonMount;
    public MeshInstance3D LoadedCannonBall;

    public float SplashRadius { get; set; } = 1; // 1 m

    CollisionShape3D RangeHitbox;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.TowerType = TowerType.Cannon;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        System.Collections.Generic.Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 2.5f},
            {StatType.Damage, 550.0f},
            {StatType.Range, 14.0f},
        };
        StatBlock.SetStatBlock(sb);

        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
        CannonMount = GetNode<MeshInstance3D>("weapon_cannon2/tmpParent/weapon_cannon");
        RangeHitbox = ActiveRange.GetNode<CollisionShape3D>("CollisionShape3D");
        LoadedCannonBall = GetNode<MeshInstance3D>("weapon_cannon2/tmpParent/weapon_cannon/cannon/CannonBall");
        LoadedCannonBall.Visible = false;
    }

    private void MoveCannonballs(double delta)
    {
        foreach (MeshInstance3D cannonBall in CannonBalls)
        {
            cannonBall.TranslateObjectLocal(new Vector3(0, 0, -70.2f * (float)delta));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        MoveCannonballs(delta);

        base._Process(delta);
        if (Disabled) return;

        if (EnemyList.Count > 0)
        {
            int index = EnemyList
            .Select((item, index) => new { Item = item, Index = index, Progress = item.GetProgress() })
            .OrderBy(x => x.Progress)
            .First()
            .Index;

            if (index != -1)
            {
                CannonMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    LaunchCannon(this, EnemyList[index]); // this instead.

                    GetNode<AudioStreamPlayer3D>("FiringSound").Play();
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

    public void DealDamage(Area3D area)
    {
        BaseEnemy be = area.GetParent<BaseEnemy>();
        be.TakeDamage(StatBlock.GetStat(StatType.Damage), this);

        // Apply splash damage to nearby enemies
        ApplySplashDamage(be.GlobalPosition, this.SplashRadius, StatBlock.GetStat(StatType.Damage) * 0.5f, be);

        be.StrikeSound.Play();
    }

    private void ApplySplashDamage(Vector3 explosionPosition, float radius, float damage, BaseEnemy directHitEnemy)
    {
        var spaceState = GetTree().Root.GetNode<GameLoop>("SceneSwitcher/GameLoop").GetWorld3D().DirectSpaceState;

        // Create the sphere shape
        var sphereShape = new SphereShape3D
        {
            Radius = radius
        };

        // Set up the query parameters
        var queryParams = new PhysicsShapeQueryParameters3D
        {
            CollideWithAreas = true,
            Shape = sphereShape,
            Transform = Transform3D.Identity.Translated(explosionPosition),
            CollisionMask = 1
        };

        // Perform the collision query
        var results = spaceState.IntersectShape(queryParams, 32); // Adjust the max result count as needed

        // Deal splash damage to each enemy found
        foreach (Dictionary result in results)
        {
            Node3D node = (Node3D)result["collider"];
            if (node.GetParent() is BaseEnemy enemy && enemy != directHitEnemy)
            {
                if (Debugging) GD.Print("splash to " + node.Name);
                enemy.TakeDamage(damage, enemy);
            }
        }
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

    public void LaunchCannon(Node3D tower, Node3D target)
    {
        Cannon cannon = tower as Cannon;
        cannon._animationPlayer.Play("launch_cannonball");


        MeshInstance3D cannonBall = CannonBallScene.Instantiate<MeshInstance3D>();
        cannonBall.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            if (!cannonBall.HasMeta("struck"))
            {
                cannonBall.SetMeta("struck", "");

                // Deal full damage to the first target
                cannonBall.GetNode<Area3D>("Hitbox").AreaEntered -= ((Cannon)tower).DealDamage;

                ParticleSignals.GetInstance().createParticle("CandyParticle", cannonBall.GlobalPosition, cannonBall.GlobalRotation);
            }
            CannonBalls.Remove(cannonBall);
            cannonBall.QueueFree();
        };

        SceneSwitcher.root.AddChild(cannonBall);
        Timer t = new Timer();
        cannonBall.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => {
            if (cannonBall != null && !cannonBall.IsQueuedForDeletion())
            {
                CannonBalls.Remove(cannonBall);
                cannonBall.QueueFree();
            }
            t.QueueFree();
        };

        cannonBall.GetNode<Area3D>("Hitbox").AreaEntered += ((Cannon)tower).DealDamage;
        cannonBall.GlobalPosition = LoadedCannonBall.GlobalPosition;
        cannonBall.LookAt(target.GlobalPosition + new Vector3(0, 0.5f, 0));

        CannonBalls.Add(cannonBall);
        cannonBall.SetMeta("target", target.GetPath());
    }
}
