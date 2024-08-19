using Godot;
using System;

public partial class ConfusedEffect : BaseStatusEffect
{
    private int Level { get; set; } = 1;
    private string effectIconPath = "res://Assets/Icons/StatusEffects/Debuffs/ConfusedIcon.tres";
    private float OriginalSpeed { get; set; }


    public override void _Ready()
    {
        this.EffectName = "Confused";
        this.ApplicationInterval = 0.001f;
        this.ApplyOnce = true;
        SetConfuseAmountFromLevel();
        base._Ready();
        this.OriginalSpeed = this.enemy.StatBlock.GetStat(StatType.Speed);
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
    }

    private void SetConfuseAmountFromLevel()
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

    public override void ApplyEffect()
    {
        float confusedSpeed = -this.OriginalSpeed;
        GD.Print($"Original Speed: {this.OriginalSpeed}, Confused Speed: {confusedSpeed}");
        this.enemy.StatBlock.SetStat(StatType.Speed, confusedSpeed);
        //this.enemy.RotateY(Mathf.DegToRad(90));
    }

    public override void EndEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.Speed, this.OriginalSpeed);
        //this.enemy.RotateY(Mathf.DegToRad(90));

        base.EndEffect();
    }
}
