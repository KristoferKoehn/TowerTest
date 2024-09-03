using Godot;
using MMOTest.Backend;
using System;

public partial class SlowedEffect : BaseStatusEffect
{
    private int Level { get; set; } = 1;
    private float AmountToSlowTo { get; set; }
    private string effectIconPath = "res://Assets/Icons/StatusEffects/Debuffs/SlowedIcon.tres";
    private float OriginalSpeed { get; set; }

    public override void _Ready()
    {
        this.EffectName = "Slowed";
        this.ApplicationInterval = 0.001f;
        this.ApplyOnce = true;
        SetPropertiesFromLevel();
        base._Ready();
        this.OriginalSpeed = this.enemy.StatBlock.GetStat(StatType.Speed);
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
    }

    public override void SetPropertiesFromLevel()
    {
        switch (Level)
        {
            case 1:
                this.AmountToSlowTo = 0.5f; // Reduce speed to 50%
                this.TotalDuration = 10.0f;
                break;
            case 2:
                this.AmountToSlowTo = 0.3f; // Reduce Speed to 30%
                this.TotalDuration = 20.0f;
                break;
            case 3:
                this.AmountToSlowTo = 0.15f; // Reduce Speed to 15%
                this.TotalDuration = 30.0f;
                break;
        }
    }

    public override void ApplyEffect()
    {
        // Apply the slow effect by multiplying the original speed with the slow amount
        float slowedSpeed = this.OriginalSpeed * this.AmountToSlowTo;
        GD.Print($"Original Speed: {this.OriginalSpeed}, Slowed Speed: {slowedSpeed}");
        this.enemy.StatBlock.SetStat(StatType.Speed, slowedSpeed);
    }

    public override void EndEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.Speed, this.OriginalSpeed);
        base.EndEffect();
    }
}
