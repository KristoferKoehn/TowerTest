using Godot;
using System;
using System.Collections.Generic;

public partial class DoubleTowerSpeed : BaseAction
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Level = 1;
        this.SetAttributesFromLevel();
		this.ActionName = "DoubleTowerSpeed";
		this.Description = $"Doubles the attack speed stat of all currently placed towers for a duration of {this.DurationSeconds} seconds.";
		this.IsTemporary = true;
		base._Ready();
	}

    // Method to apply the action's effect
    public override void ApplyEffect()
    {
		foreach (List<AbstractTower> towerList in TowerManager.GetInstance().activeTowers.Values)
		{
			foreach (AbstractTower tower in towerList)
			{
                tower.StatBlock.SetStat(StatType.AttackSpeed, tower.StatBlock.GetStat(StatType.AttackSpeed) / 2);
            }
        }
        base.ApplyEffect(); // starting duration timer, etc.
    }

    // Method to end the action's effect
    public override void EndEffect()
    {
        foreach (List<AbstractTower> towerList in TowerManager.GetInstance().activeTowers.Values)
        {
            foreach (AbstractTower tower in towerList)
            {
                tower.StatBlock.SetStat(StatType.AttackSpeed, tower.StatBlock.GetStat(StatType.AttackSpeed) * 2);
            }
        }
        base.EndEffect(); // queuefreeing, etc.
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
