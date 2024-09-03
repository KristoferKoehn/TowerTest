using Godot;
using System;
using TowerTest.Scenes.Components;

public partial class BaseAction : AbstractPlaceable
{
    private bool Debugging = false;
    // Action name
    public string ActionName { get; set; }

    // Description of the action
    public string Description { get; set; }

    // IsTemporary or not
    public bool IsTemporary { get; set; }

    // DurationSeconds of the effect (for temporary actions)
    public float DurationSeconds { get; set; }

    // Indicates if the action is currently active
    public bool Disabled { get; private set; }

    public int Level { get ; set; }

    private Timer durationTimer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (IsTemporary)
        {
            durationTimer = new Timer();
            durationTimer.Autostart = false;
            if(Debugging) GD.Print(DurationSeconds);
            durationTimer.WaitTime = this.DurationSeconds;
            durationTimer.Timeout += () => EndEffect();
            AddChild(durationTimer);
        }
    }

    // Method to apply the action's effect
    public virtual void ApplyEffect()
    {
        this.Visible = false;
        if (Debugging) GD.Print($"{ActionName} effect applied.");

        if (IsTemporary)
        {
            durationTimer.Start();
        }
        else
        {
            EndEffect();
        }
    }

    // Method to end the action's effect
    public virtual void EndEffect()
    {
        ActionManager.GetInstance().activeActions.Remove(this);
        QueueFree();
        if(Debugging) GD.Print($"{ActionName} effect ended.");
    }

    internal virtual void SetAttributesFromLevel()
    {
    }

    public override void DisplayMode()
    {
        Disabled = true;
    }

    public override void ActivatePlacing()
    {
    }
}
