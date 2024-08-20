using Godot;
using System;

public partial class DoubleTowerSpeed : BaseArtifact
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        this.SetAttributesFromLevel();
		this.ArtifactName = "DoubleTowerSpeed";
		this.Description = $"Doubles the attack speed stat of all currently placed towers for a duration of {this.DurationSeconds} seconds.";
		this.IsTemporary = true;
		base._Ready();
	}

    // Method to apply the artifact's effect
    public override void ApplyEffect()
    {
		foreach (Ballista ballista in BallistaArrowManager.GetInstance().ballistas)
		{
			ballista.StatBlock.SetStat(StatType.AttackSpeed, ballista.StatBlock.GetStat(StatType.AttackSpeed) * 2);
		}
        base.ApplyEffect();
    }

    // Method to end the artifact's effect
    public override void EndEffect()
    {
        foreach (Ballista ballista in BallistaArrowManager.GetInstance().ballistas)
        {
            ballista.StatBlock.SetStat(StatType.AttackSpeed, ballista.StatBlock.GetStat(StatType.AttackSpeed) / 2);
        }
        base.EndEffect();
    }

    internal override void SetAttributesFromLevel()
    {
		switch(Level)
		{
			case 1:
				this.DurationSeconds = 10;
				break;
			case 2:
				this.DurationSeconds = 20;
				break;
			case 3:
				this.DurationSeconds = 30;
				break;
		}
    }
}
