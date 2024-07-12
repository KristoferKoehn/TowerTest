using Godot;
using MMOTest.Backend;
using System;

public partial class HealthBar : Sprite3D
{
    private ProgressBar healthBar;
    private ProgressBar armorBar;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

        healthBar = GetNode<ProgressBar>("SubViewport/HealthBar");
        armorBar = GetNode<ProgressBar>("SubViewport/ArmorBar");
        
        BaseEnemy be = GetParent<BaseEnemy>();
        healthBar.MaxValue = be.StatBlock.GetStat(StatType.MaxHealth);
        armorBar.MaxValue = be.StatBlock.GetStat(StatType.MaxArmor);

        healthBar.Value = be.StatBlock.GetStat(StatType.Health);
        armorBar.Value = be.StatBlock.GetStat(StatType.Armor);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public void UpdateHealthBar(Node enemy2, Node source)
    {
        BaseEnemy enemy = enemy2 as BaseEnemy;
        // Update the health bar's value
        healthBar.MaxValue = enemy.StatBlock.GetStat(StatType.MaxHealth);
        armorBar.MaxValue = enemy.StatBlock.GetStat(StatType.MaxArmor);

        healthBar.Value = enemy.StatBlock.GetStat(StatType.Health);
        armorBar.Value = enemy.StatBlock.GetStat(StatType.Armor);

    }
}
