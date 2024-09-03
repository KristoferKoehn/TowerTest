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
        SetPropertiesFromLevel();
        base._Ready();
        this.OriginalSpeed = this.enemy.StatBlock.GetStat(StatType.Speed);
        this.effectIcon.Texture = ResourceLoader.Load<AtlasTexture>(effectIconPath);
    }

    public override void ApplyEffect()
    {
        float confusedSpeed = -this.OriginalSpeed;
        GD.Print($"Original Speed: {this.OriginalSpeed}, Confused Speed: {confusedSpeed}");
        this.enemy.StatBlock.SetStat(StatType.Speed, confusedSpeed);
        this.enemy.GetNode<Node3D>("Rig").RotateY(Mathf.Pi);
    }

    public override void EndEffect()
    {
        this.enemy.StatBlock.SetStat(StatType.Speed, this.OriginalSpeed);
        this.enemy.GetNode<Node3D>("Rig").RotateY(Mathf.Pi);

        base.EndEffect();
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
}
