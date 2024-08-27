using Godot;
using System;

public partial class UpgradeCardsInHand : BaseAction
{
    public int NumberOfCardsToUpgrade { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.SetAttributesFromLevel();
        this.ActionName = "UpgradeCardsInHand";
        this.Description = $"Upgrades a number of cards in your hand, gives you the choice of which to upgrade.";
        this.IsTemporary = false;
        base._Ready();
    }

    // Method to apply the actions's effect
    public override void ApplyEffect()
    {

        base.ApplyEffect();
    }

    // Method to end the action's effect
    public override void EndEffect()
    {
        base.EndEffect();
    }

    internal override void SetAttributesFromLevel()
    {
        switch (Level)
        {
            case 1:
                this.NumberOfCardsToUpgrade = 1;
                break;
            case 2:
                this.NumberOfCardsToUpgrade = 2;
                break;
            case 3:
                this.NumberOfCardsToUpgrade = 3;
                break;
        }
    }

}
