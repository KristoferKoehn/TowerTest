using Godot;
using System;

public partial class BurnedEffect : BaseStatusEffect
{
    private int Level { get; set; } = 1;
    private float Damage { get; set; }

    private string effectIconPath = "res://Assets/Icons/StatusEffects/Debuffs/BurnedIcon.tres";

    public override void _Ready()
    {
        this.EffectName = "Burned";
        this.ApplicationInterval = 0.2f; // burn damage is almost constantly applied.
        SetDamageFromLevel();
        base._Ready();
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
    }

    private void SetDamageFromLevel()
    {
        switch (Level)
        {
            case 1:
                this.Damage = 10f;
                this.TotalDuration = 10.0f;
                break;
            case 2:
                this.Damage = 20f;
                this.TotalDuration = 20.0f;
                break;
            case 3:
                this.Damage = 30f;
                this.TotalDuration = 30.0f;
                break;
        }
    }

    public override void ApplyEffect()
    {
        this.enemy.TakeDamage(Damage, this);
    }

    // We wont have to do anything with removing the effect because we aren't changing stats, so we dont have to reset them.
}
