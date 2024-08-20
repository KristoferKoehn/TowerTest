using Godot;
using System;
using TowerTest.Scenes.Components;
public partial class BaseArtifact : AbstractPlaceable
{
    // Artifact name
    public string ArtifactName { get; set; }

    // Description of the artifact
    public string Description { get; set; }

    // IsTemporary or not
    public bool IsTemporary { get; set; }

    // DurationSeconds of the effect (for temporary artifacts)
    public float DurationSeconds { get; set; }

    // Indicates if the artifact is currently active
    public bool Disabled { get; private set; }

    public int Level { get ; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!Disabled && IsTemporary)
        {
            // Decrease the duration over time
            DurationSeconds -= (float)delta;

            if (DurationSeconds <= 0)
            {
                EndEffect();
            }
        }
    }

    // Method to apply the artifact's effect
    public virtual void ApplyEffect()
    {
        GD.Print($"{ArtifactName} effect applied.");
        Disabled = true;

        if (IsTemporary)
        {
            // Start duration countdown if the artifact is temporary
            _Process(0);
        }
    }

    // Method to end the artifact's effect
    public virtual void EndEffect()
    {
        ArtifactManager.GetInstance().activeArtifacts.Remove(this);
        GD.Print($"{ArtifactName} effect ended.");
        Disabled = false;
        QueueFree(); // Optionally free the artifact node after the effect ends
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
        Visible = false;

    }
}

