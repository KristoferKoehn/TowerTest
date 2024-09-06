using Godot;
using System;

public abstract partial class BaseStatusEffect : Node
{
    public Timer totalDurationTimer;
    public Timer applicationTimer;
    public TextureRect effectIcon;

    // Common properties for all status effects
    public float ApplicationInterval { get; set; } = 0.1f;
    public string EffectName { get; set; } = string.Empty;
    public float TotalDuration { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool ApplyOnce { get; set; } = false;
    public int Level { get; set; } = 1;

    internal BaseEnemy enemy;

    public override void _Ready()
    {
        // Adding Timers and the Icon:
        this.totalDurationTimer = new Timer();
        this.AddChild(totalDurationTimer);
        this.applicationTimer = new Timer();
        this.AddChild(applicationTimer);
        this.effectIcon = new TextureRect();
        this.effectIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        this.effectIcon.CustomMinimumSize = new Vector2(50, 50);

        // Setting Properties:
        this.Name = EffectName; // ActionName the node (not really necessary but when you look at remote it will show)
        this.enemy = this.GetParent<BaseEnemy>(); // The effect should be added as a child to the enemy. enemy doesnt have to know about the effect

        // Timer and Event Set Up:
        this.totalDurationTimer.WaitTime = this.TotalDuration;
        this.applicationTimer.WaitTime = this.ApplicationInterval;
        this.totalDurationTimer.Timeout += () => EndEffect();
        this.applicationTimer.Timeout += () => ApplyEffect();
        this.applicationTimer.OneShot = this.ApplyOnce;

        // Adding the Icon to the enemy, and starting the Timers:
        this.enemy.healthBar.gridContainer.AddChild(this.effectIcon);
        this.totalDurationTimer.Start();
        this.applicationTimer.Start();
    }

    // Method to apply the effect (to be overridden by derived classes)
    public abstract void ApplyEffect();

    // Method to end the effect
    public virtual void EndEffect()
    {
        this.enemy.RemoveStatusEffect(this);
    }

    public abstract void SetPropertiesFromLevel();
}
