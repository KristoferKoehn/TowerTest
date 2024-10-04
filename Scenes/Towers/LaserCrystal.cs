using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LaserCrystal : AbstractTower
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        this.DamageType = DamageType.Physical;
        this.TowerType = TowerType.LaserCrystal;
        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 0.0666f},
            {StatType.Damage, 8.0f},
            {StatType.Range, 8.0f},
            {StatType.CritRate, 10.0f },
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
            .OrderByDescending(x => x.Progress)
            .First()
            .Index;

            if (index != -1)
            {
                GetNode<Node3D>("LaserGimbal").Visible = true;
                MeshInstance3D laser = GetNode<MeshInstance3D>("LaserGimbal/Laser");
                GetNode<Node3D>("LaserGimbal").LookAt(EnemyList[index].GlobalPosition + new Vector3(0, 0.3f, 0));
                float LaserLength = (this.GlobalPosition - EnemyList[index].GlobalPosition).Length();
                ((CylinderMesh)laser.Mesh).Height = LaserLength;
                laser.Position = new Vector3(0, 0, -LaserLength/2f);
                if(CanShoot)
                {
                    CanShoot = false;
                    EmitSignal("TowerFired", this, EnemyList[index]);
                    EnemyList[index].TakeDamage(StatBlock.GetStat(StatType.Damage), this, false, this.DamageType);
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed) / this.TimeScale);
                }
            }
        }
        else
        {
            GetNode<Node3D>("LaserGimbal").Visible = false;
        }

        float radius = StatBlock.GetStat(StatType.Range);
        ((CylinderShape3D)RangeHitbox.Shape).Radius = radius;

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

}
