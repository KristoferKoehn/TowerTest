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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void UpdateHealthBar(Node enemy2, Node source)
    {
        BaseEnemy enemy = enemy2 as BaseEnemy;
        // Update the health bar's value
        healthBar.Value = enemy.currentHealth / enemy.StatBlock.GetStat(StatType.Health) * 100;
        armorBar.Value = enemy.currentArmor / enemy.StatBlock.GetStat(StatType.Armor) * 100;
    }
}
