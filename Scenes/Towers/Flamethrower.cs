using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class Flamethrower : AbstractTower
{
    [Export] MeshInstance3D TowerBase;
    [Export] Node3D FlamethrowerMount;
    [Export] Area3D FlameArea;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.DamageType = DamageType.Fire;
        this.TowerType = TowerType.Flamethrower;
        base._Ready();
        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 1.0f},
            {StatType.Damage, 10.0f},
            {StatType.Range, 7.0f},
            {StatType.CritRate, 5.0f },
        };
        StatBlock.SetStatBlock(sb);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
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
                FlamethrowerMount.LookAt(EnemyList[index].GlobalPosition);
                if (CanShoot)
                {
                    CanShoot = false;
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));

                    //EmitSignal("TowerFired", this, EnemyList[index]);
                    ShootFlame(this, EnemyList[index]); // this instead.
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

    public void ShootFlame(Node3D tower, Node3D target)
    {
    }
}
