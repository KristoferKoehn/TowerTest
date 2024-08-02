using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Control
{
    //private static readonly Vector2 CardSize = new Vector2(125, 175);
    private static PackedScene BaseCardScene;
    private Curve CardCurveWidth = GD.Load<Curve>("res://Scenes/UI/Cards/CardCurveWidth.tres");
    private Curve CardCurveHeight = GD.Load<Curve>("res://Scenes/UI/Cards/CardCurveHeight.tres");
    private Curve CardCurveRotation = GD.Load<Curve>("res://Scenes/UI/Cards/CardCurveRotation.tres");
    private List<BaseCard> CardList = new List<BaseCard>();
    private TextureRect PlaceholderTextureRect;

    [Export]
    private float HAND_WIDTH_SCALE = 500f;
    [Export]
    private float HAND_HEIGHT_SCALE = 20f;
    [Export]
    private float HAND_ROTATION_SCALE = 5f;

    public override void _Process(double delta)
    {
        HAND_WIDTH_SCALE = this.Size.X;
        HAND_HEIGHT_SCALE = this.Size.Y;
        UpdateCardPositions();
    }
    public override void _Ready()
    {
        //this.Position = this.GetViewport().GetVisibleRect().Size / 2;
        BaseCardScene = GD.Load<PackedScene>("res://Scenes/UI/Cards/BaseCard.tscn");
        PlaceholderTextureRect = GetNode<TextureRect>("TextureRect");
        PlaceholderTextureRect.Visible = false;
        // Generate a test hand:
        this.GenerateHandWithAllCards();
    }

    public void AddCard(BaseCard card)
    {
        this.AddChild(card);
        //card.AnchorsPreset = (int)Control.LayoutPreset.Center;
        //card.SetAnchorsPreset(LayoutPreset.Center);
        this.CardList.Add(card);

        //Subscribe the card to clicked event:
        card.GuiInput += (InputEvent @event) => {
            if (@event.IsAction("select"))
            {
                OnChunkCardClicked(@event, card);
            }

        };

        UpdateCardPositions();
    }

    private void UpdateCardPositions()
    {
        //GD.Print("Updating card positions...");

        for (int i = 0; i < CardList.Count; i++)
        {
            BaseCard card = CardList[i];
            float hand_ratio = 0.5f;
            if (CardList.Count > 1)
            {
                hand_ratio = (float)i / (float)(CardList.Count - 1);
            }

            // Sample curves for position and rotation
            float widthOffset = CardCurveWidth.Sample(hand_ratio) * HAND_WIDTH_SCALE;
            float heightOffset = CardCurveHeight.Sample(hand_ratio) * HAND_HEIGHT_SCALE;
            float rotationOffset = CardCurveRotation.Sample(hand_ratio) * HAND_ROTATION_SCALE;

            // Debugging outputs
            //GD.Print($"Card {i}: hand_ratio = {hand_ratio}");
            //GD.Print($"Card {i}: widthOffset = {widthOffset}, heightOffset = {heightOffset}, rotationOffset = {rotationOffset}");

            // Update position relative to parent (PlayerHand)
            card.Position = new Vector2(widthOffset, -heightOffset);

            // Update rotation
            card.RotationDegrees = rotationOffset;
        }
    }

    // When a chunk card is clicked:
    public void OnChunkCardClicked(InputEvent @event, BaseCard card)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GD.Print("Clicked on a card.");

            if (card != null)
            {
                // Load and instantiate the card:
                Chunk newchunk = GD.Load<PackedScene>(card.ScenePath).Instantiate<Chunk>();
                newchunk.CurrentlyPlacing = true;
                newchunk.Debug = true;
                this.Visible = false;
                this.AddChild(newchunk); // adding chunk to player hand? could change what we add the node to.
            }
        }
    }

    // Fills the player hand with one of every chunk card possible.
    private void GenerateHandWithAllCards()
    {
        this.CardList.Clear();
        foreach (string chunk in CardDatabase.chunkslist)
        {
            BaseCard chunkCard = BaseCardScene.Instantiate<BaseCard>();
            chunkCard.SetCard(chunk);
            AddCard(chunkCard);
        }
    }
}
