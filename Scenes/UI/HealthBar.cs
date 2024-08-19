using Godot;
using MMOTest.Backend;
using System;

public partial class HealthBar : Sprite3D
{
    [Export] public ProgressBar healthBar;
    [Export] public ProgressBar armorBar;
    [Export] public GridContainer gridContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
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

    private void AddStatusEffectIcon(string icon) // change it to (StatusEffect status) when possible
    {
        TextureRect textrect = new TextureRect();
        textrect.CustomMinimumSize = new Vector2(50,50);
        textrect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        AtlasTexture effecticon = (AtlasTexture)ResourceLoader.Load<Resource>("res://Assets/Icons/StatusEffects/Debuffs/BurnedIcon.tres");
        textrect.Texture = effecticon;
        this.gridContainer.AddChild(textrect);
    }

    private void RemoveStatusEffectIcon(string icon) // change it to (StatusEffect status) when possible
    {
        foreach (Node child in gridContainer.GetChildren())
        {
            if (child is TextureRect textrect && textrect.Texture.ResourcePath.Contains(icon))
            {
                gridContainer.RemoveChild(textrect);
                textrect.QueueFree(); // Ensure the node is freed from memory
                break; // Exit the loop once the icon is found and removed
            }
        }
    }
}
