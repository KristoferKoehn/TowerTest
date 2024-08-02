using Godot;
using System;
using System.Collections.Generic;

public partial class GravityCrystal : AbstractTower
{
    [Export]
    MeshInstance3D EnergyBand;

    [Export]
    GpuParticles3D AmbientParticles;
    [Export]
    GpuParticles3D AttackParticles;
    [Export]
    AudioStreamPlayer3D FiringSound;

    ShaderMaterial EnergyBandMaterial;

    float LastRange;

    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        base._Ready();
        Dictionary<StatType, float> sb = new()
        {
            {StatType.AttackSpeed, 1.3f},
            {StatType.Damage, 50.0f},
            {StatType.Range, 4.0f},
        };
        StatBlock.SetStatBlock(sb);

        EnergyBandMaterial = (ShaderMaterial)EnergyBand.Mesh.SurfaceGetMaterial(0);

        LastRange = StatBlock.GetStat(StatType.Range);

        EnergyBandMaterial.SetShaderParameter("Alpha", 0.0f);

        ShotTimer.Timeout += ShotTimerTimeout;
        ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        base._Process(delta);
        if (LastRange != StatBlock.GetStat(StatType.Range))
        {
            LastRange = StatBlock.GetStat(StatType.Range);
            CylinderMesh cylinderMesh = (CylinderMesh)EnergyBand.Mesh;
            float range = StatBlock.GetStat(StatType.Range);
            cylinderMesh.BottomRadius = range;
            cylinderMesh.TopRadius = range + 0.4f;
            ((ParticleProcessMaterial)AmbientParticles.ProcessMaterial).EmissionRingRadius = range;
        }

        if (MouseOver || Selected)
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

        if (EnemyList.Count > 0) {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(EnergyBandMaterial, "shader_parameter/Alpha", 1f, 0.2);
            
            if (CanShoot)
            {
                CanShoot = false;
                AttackParticles.Emitting = true;
                GetTree().CreateTimer(0.14).Timeout += DamageEnemies;
            }

        } else
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(EnergyBandMaterial, "shader_parameter/Alpha", 0f, 0.2);
        }


    }

    public void ShotTimerTimeout()
    {
        CanShoot = true;
        ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed));
    }

    public void DamageEnemies()
    {
        EmitSignal("TowerFired", this);
        FiringSound.Play();
        foreach (BaseEnemy be in EnemyList)
        {
            be.TakeDamage(StatBlock.GetStat(StatType.Damage), this);
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

}
