using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Flamethrower : AbstractTower
{
    [Export] private MeshInstance3D TowerBase;
    [Export] private Node3D FlamethrowerMount;
    [Export] private Area3D FlameArea;
    [Export] private GpuParticles3D FlameParticles;

    private PackedScene BurnedEffectScene = GD.Load<PackedScene>("res://Scenes/StatusEffects/BurnedEffect.tscn");

    private HashSet<BaseEnemy> enemiesInFlameArea = new();

    public override void _Ready()
    {
        this.DamageType = DamageType.Fire;
        this.TowerType = TowerType.Flamethrower;
        base._Ready();

        Dictionary<StatType, float> sb = new()
        {
            { StatType.AttackSpeed, 0.1f },
            { StatType.Damage, 0.5f },
            { StatType.Range, 7.0f },
            { StatType.CritRate, 5.0f },
        };
        StatBlock.SetStatBlock(sb);

        FlameParticles.Emitting = false;

        FlameArea.AreaEntered += OnFlameAreaBodyEntered;
        FlameArea.AreaExited += OnFlameAreaBodyExited;
        ShotTimer.Timeout += DamageEnemiesInFlame;
    }

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
                    ShotTimer.Start(StatBlock.GetStat(StatType.AttackSpeed) / this.TimeScale);

                    FlameParticles.Emitting = true;

                    FiringSound.Play();
                }
            }
        }
        else
        {
            FlameParticles.Emitting = false;
        }

        // Update the range of the hitbox
        ((CylinderShape3D)RangeHitbox.Shape).Radius = StatBlock.GetStat(StatType.Range);

        // Handle visual indication for range and placement
        if (MouseOver || Selected || Placing)
        {
            DrawRangeIndicator();
        }
    }

    private void OnFlameAreaBodyEntered(Node body)
    {
        if (body.GetParent() is BaseEnemy enemy)
        {
            GD.Print("Flame entered");
            enemiesInFlameArea.Add(enemy);
        }
    }

    private void OnFlameAreaBodyExited(Node body)
    {
        if (body.GetParent() is BaseEnemy enemy)
        {
            GD.Print("Flame exited");
            enemiesInFlameArea.Remove(enemy);
        }
    }

    private void DamageEnemiesInFlame()
    {
        foreach (var be in enemiesInFlameArea)
        {
            bool isCrit = false;

            // Get the crit rate and clamp it to 100%
            float critRate = this.StatBlock.GetStat(StatType.CritRate);
            critRate = Math.Min(critRate, 100f); // Ensure crit rate does not exceed 100%

            Random rand = new Random();
            int randomNum = rand.Next(0, 100);

            // Determine if the attack is a critical hit
            if (randomNum < critRate)
            {
                isCrit = true;
            }

            // Calculate damage
            float normalDamage = StatBlock.GetStat(StatType.Damage);
            float critMultiplier = 1.5f; // Assuming CritMultiplier is a multiplier (e.g., 1.5x for 50% bonus)
            float damage = normalDamage;

            if (isCrit)
            {
                damage *= critMultiplier; // Apply crit multiplier if it's a crit
                BurnedEffect burn = BurnedEffectScene.Instantiate<BurnedEffect>();
                burn.Level = 1;
                be.AddStatusEffect(burn);
            }

            // Apply damage to the be
            be.TakeDamage(damage, this, isCrit, this.DamageType);
            be.StrikeSound.Play();
        }
    }

    private void DrawRangeIndicator()
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
