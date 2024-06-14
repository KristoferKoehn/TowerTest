using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Node2D
{
    private static readonly Vector2 CardSize = new Vector2(125, 175);
    private static PackedScene CardBase;
    private static PackedScene PlayerHandScene;
    private List<Node> CardSelected = new List<Node>();
    private int DeckSize;
    private Vector2 CenterCardOval;
    private float HorRad;
    private float VertRad;
    private float Angle = Mathf.DegToRad(90) - 0.5f;
    private Vector2 OvalAngleVector;

    /*
    public override void _Ready()
    {
        CardBase = GD.Load<PackedScene>("res://Scenes/UI/BaseCard.tscn");
        PlayerHandScene = GD.Load<PackedScene>("res://Scenes/UI/PlayerHand.tscn");
        DeckSize = PlayerHand.CardList.size(); // Replace `YourCardListType` with the appropriate type
        CenterCardOval = GetViewport().Size * new Vector2(0.5f, 1.25f);
        HorRad = GetViewport().Size.x * 0.45f;
        VertRad = GetViewport().Size.y * 0.4f;
    }

    public void DrawCard()
    {
        var NewCard = (Node2D)CardBase.Instantiate();
        int cardSelectedIndex = (int)GD.Randi() % DeckSize;
        var playerHand = PlayerHandScene.Instance();
        var cardList = playerHand.GetNode<YourCardListType>("CardList"); // Replace `YourCardListType` with the appropriate type

        OvalAngleVector = new Vector2(HorRad * Mathf.Cos(Angle), -VertRad * Mathf.Sin(Angle));
        NewCard.Position = CenterCardOval + OvalAngleVector - CardSize / 2;
        NewCard.Scale = CardSize / NewCard.Size;
        NewCard.Rotation = (90 - Mathf.RadToDeg(Angle)) / 4;
        AddChild(NewCard);
        cardList.RemoveAt(cardSelectedIndex);
        Angle += 0.25f;
        DeckSize -= 1;
    }
    */
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
