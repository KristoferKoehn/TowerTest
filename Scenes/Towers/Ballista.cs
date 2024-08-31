using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ballista : AbstractTower
{
    PackedScene ArrowScene = GD.Load<PackedScene>("res://Scenes/Components/BalistaArrow.tscn");
    public List<MeshInstance3D> Arrows = new();

    MeshInstance3D TowerBase;
    MeshInstance3D BallistaMount;
    MeshInstance3D BallistaBow;
    MeshInstance3D Arrow;

    CollisionShape3D RangeHitbox;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.TowerType = TowerType.Ballista;
        base._Ready();
        if (!Disabled)
        {
            TowerManager.GetInstance().RegisterTower(this);
        }

        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 1.0f},
            {StatType.Damage, 201.0f},
            {StatType.Range, 7.0f},
        };
        StatBlock.SetStatBlock(sb);

        TowerBase = GetNode<MeshInstance3D>("towerSquare_bottomA2/tmpParent/towerSquare_bottomA");
        BallistaMount = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista");
        BallistaBow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow");
        Arrow = GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow");
        RangeHitbox = ActiveRange.GetNode<CollisionShape3D>("CollisionShape3D");
    }

    private void MoveArrows(double delta)
    {
        foreach (MeshInstance3D arrow in Arrows)
        {
            arrow.TranslateObjectLocal(new Vector3(0, 0, -80.2f * (float)delta));
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        MoveArrows(delta);

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
                Vector3 predicted = PredictionManager.GetInstance().PredictTarget(EnemyList[index], 1.2f);
                BallistaMount.LookAt(predicted);
                //BallistaMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));

                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    ShootArrow(this, EnemyList[index], predicted); // this instead.

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
    public void ShootArrow(Node3D tower, Node3D target, Vector3 dir)
    {
        //make arrow at tower (in the right spot)
        //
        MeshInstance3D arrow = ArrowScene.Instantiate<MeshInstance3D>();
        arrow.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            //overdamage protection, prevents damage being dealt to multiple enemies erroneously 
            if (!arrow.HasMeta("struck"))
            {
                arrow.SetMeta("struck", "");
                arrow.GetNode<Area3D>("Hitbox").AreaEntered -= ((Ballista)tower).DealDamage;
                ParticleSignals.GetInstance().createParticle("Particle1", arrow.GlobalPosition, arrow.GlobalRotation);
            }
            Arrows.Remove(arrow);
            arrow.QueueFree();
        };
        SceneSwitcher.root.AddChild(arrow);
        Timer t = new Timer();
        arrow.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => {
            if (arrow != null && !arrow.IsQueuedForDeletion())
            {
                Arrows.Remove(arrow);
                arrow.QueueFree();
            }
            t.QueueFree();
        };
        arrow.GetNode<Area3D>("Hitbox").AreaEntered += ((Ballista)tower).DealDamage;
        arrow.GlobalPosition = tower.GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow").GlobalPosition;
        arrow.LookAt(dir + new Vector3(0, 0.5f, 0));

        Arrows.Add(arrow);
        arrow.SetMeta("target", target.GetPath());
    }

    public void DealDamage(Area3D area)
    {
        BaseEnemy be = area.GetParent<BaseEnemy>();
        be.TakeDamage(StatBlock.GetStat(StatType.Damage), this);
        be.StrikeSound.Play();
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
