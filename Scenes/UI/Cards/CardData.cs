using Godot;

[GlobalClass]
public partial class CardData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string CardInfo { get; set; }
    [Export] public string Rarity { get; set; } = "Common";
    [Export] public int ShopCost { get; set; }
    [Export] public string SubjectScene { get; set;}
    [Export] public float FOV { get; set; } = 75f;
    [Export] public float CameraOrbitSpeed { get; set; } = 1f;
    [Export] public float CameraTilt { get; set; } = 0;
    [Export] public float CameraZoom { get; set; } = 5.6f;
    [Export] public StatType ResourceCostType { get; set; }
    [Export] public float ResourceCostValue { get; set; }
    [Export] public CardType CardType { get; set; }
}

public enum CardType
{
    Tower,
    Chunk,
    Action
}
