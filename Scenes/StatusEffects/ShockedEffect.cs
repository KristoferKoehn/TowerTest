using Godot;
using System;

public partial class ShockedEffect : BaseStatusEffect
{
    private int Level { get; set; } = 1;
    private float Damage { get; set; }
    private float ParalyzedTime { get; set; }
    private string effectIconPath = "res://Assets/Icons/StatusEffects/Debuffs/ShockedIcon.tres";
    private float OriginalSpeed { get; set; }
    private Timer ParalyzedTimer;

    // The shocked status effect stops them every few seconds, making them immobile and doing a little damage.
    // Its like a compromise between poison and slow.
    public override void _Ready()
    {
        this.EffectName = "Shocked";
        this.ApplicationInterval = 3.0f;
        this.ApplyOnce = true;
        SetParalysisFromLevel();
        base._Ready();
        this.OriginalSpeed = this.enemy.StatBlock.GetStat(StatType.Speed);
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
        this.ParalyzedTimer = new Timer();
        this.ParalyzedTimer.WaitTime = this.ParalyzedTime;
        this.ParalyzedTimer.Timeout += () => Unparalyze();
        this.AddChild(ParalyzedTimer);
    }

    private void SetParalysisFromLevel()
    {
        switch (Level)
        {
            case 1:
                this.Damage = 50f; // Reduce speed to 50%
                this.ParalyzedTime = 1.0f;
                this.TotalDuration = 10.0f;
                break;
            case 2:
                this.Damage = 100f; // Reduce Speed to 30%
                this.ParalyzedTime = 3.0f;
                this.TotalDuration = 20.0f;
                break;
            case 3:
                this.Damage = 200f; // Reduce Speed to 15%
                this.ParalyzedTime = 5.0f;
                this.TotalDuration = 30.0f;
                break;
        }
    }

    public override void ApplyEffect()
    {
        this.applicationTimer.Stop();
        this.enemy.StatBlock.SetStat(StatType.Speed, 0.0f);
        this.enemy.TakeDamage(Damage, this);
        this.ParalyzedTimer.Start();
    }

    private void Unparalyze()
    {
        this.ParalyzedTimer.Stop();
        this.enemy.StatBlock.SetStat(StatType.Speed, OriginalSpeed);
        this.applicationTimer.Start();
    }

    public override void EndEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.Speed, this.OriginalSpeed);
        base.EndEffect();
    }
}
