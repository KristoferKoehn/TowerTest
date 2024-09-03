using Godot;
using System;

public partial class WetEffect : BaseStatusEffect
{
    private int Level { get; set; } = 1;

    private string effectIconPath = "res://Assets/Icons/StatusEffects/Debuffs/WetIcon.tres";

    public override void _Ready()
    {
        this.EffectName = "Wet";
        this.ApplicationInterval = 0.1f; // burn damage is almost constantly applied.
        this.ApplyOnce = true;
        SetPropertiesFromLevel();
        base._Ready();
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
    }

    public override void ApplyEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.ShockMultiplier, this.enemy.StatBlock.GetStat(StatType.ShockMultiplier) * 2.0f);
    }

    public override void SetPropertiesFromLevel()
    {
        switch (Level)
        {
            case 1:
                this.TotalDuration = 10.0f;
                break;
            case 2:
                this.TotalDuration = 20.0f;
                break;
            case 3:
                this.TotalDuration = 30.0f;
                break;
        }
    }

    public override void EndEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.ShockMultiplier, this.enemy.StatBlock.GetStat(StatType.ShockMultiplier) / 2.0f);
    }
}
