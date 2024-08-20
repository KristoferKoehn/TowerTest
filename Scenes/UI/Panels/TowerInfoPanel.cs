using Godot;
using System;

public partial class TowerInfoPanel : Panel
{
    private Label TypeLabel { get; set; }
    private TextureRect TypeIcon { get; set; }
    private Label LevelLabel { get; set; }
    private Label DPSLabel { get; set; }
    private Label CurrentLevelLabel { get; set; }
    private Label NextLevelLabel { get; set; }
    private Label SellCostLabel { get; set; }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        TypeLabel = GetNode<Label>("VBoxContainer/TypeInfo/ActualType");
        TypeIcon = GetNode<TextureRect>("VBoxContainer/TypeInfo/DamageTypeIcon");
        LevelLabel = GetNode<Label>("VBoxContainer/LevelInfo/ActualLevel");
        DPSLabel = GetNode<Label>("VBoxContainer/DamageInfo/ActualDPS");
        CurrentLevelLabel = GetNode<Label>("VBoxContainer/LevelUpInfo/ActualLevel");
        NextLevelLabel = GetNode<Label>("VBoxContainer/LevelUpInfo/NextLevel");
        SellCostLabel = GetNode<Label>("VBoxContainer/SellingInfo/ActualCost");
    }

    public void SetTowerInfo(AbstractTower tower)
    {
        /*
        TypeLabel.Text = tower.IsTemporary.ToString();
        TypeIcon.Texture = tower.TypeIcon;
        LevelLabel.Text = tower.Level.ToString();
        DPSLabel.Text = tower.DPS.ToString();
        CurrentLevelLabel.Text = tower.LevelUpCost.ToString();
        NextLevelLabel.Text = tower.NextLevelInfo;
        SellCostLabel.Text = tower.SellCost.ToString();
        */
    }
}
