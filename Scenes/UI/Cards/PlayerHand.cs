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
    private TextureRect HandSizeVisual;
    private Vector2 originalPosition;
    private Vector2 downPosition;

    [Export]
    Control CardPlacingPosition;

    private float HAND_WIDTH;
    private float HAND_HEIGHT;
    private float HAND_ROTATION = 5f;

    public override void _Process(double delta)
    {
    }
    public override void _Ready()
    {
        //this.PivotOffset = this.Size / 2;
        //this.Position = this.GetViewport().GetVisibleRect().Size / 2;
        BaseCardScene = GD.Load<PackedScene>("res://Scenes/UI/Cards/BaseCard.tscn");
        HandSizeVisual = GetNode<TextureRect>("TextureRect");
        HandSizeVisual.Visible = false;
        HAND_WIDTH = this.Size.X;
        HAND_HEIGHT = this.Size.Y;
        originalPosition = this.Position;
        downPosition = new Vector2(this.Position.X, this.Position.Y + 200);
        // Generate a test hand:
        this.GenerateHandWithAllCards();
        UpdateCardPositions();
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
        //this.PivotOffset = this.Size / 2; //new Vector2(this.Size.X / 2, this.Size.Y / 2);

        for (int i = 0; i < CardList.Count; i++)
        {
            BaseCard card = CardList[i];
            float hand_ratio = 0.5f;
            if (CardList.Count > 1)
            {
                hand_ratio = (float)i / (float)(CardList.Count - 1);
            }

            // Sample curves for position and rotation
            float widthOffset = CardCurveWidth.Sample(hand_ratio) * (HAND_WIDTH) / 2; // divide by 2 because it is half to left or right
            float heightOffset = CardCurveHeight.Sample(hand_ratio) * (HAND_HEIGHT);
            float rotationOffset = CardCurveRotation.Sample(hand_ratio) * HAND_ROTATION;

            // Debugging outputs
            //GD.Print($"Card {i}: hand_ratio = {hand_ratio}");
            //GD.Print($"Card {i}: widthOffset = {widthOffset}, heightOffset = {heightOffset}, rotationOffset = {rotationOffset}");

            // Update position relative to parent (PlayerHand)
            card.AnchorsPreset = (int)LayoutPreset.Center;
            card.Position = new Vector2(card.Position.X + widthOffset, card.Position.Y - heightOffset - HAND_HEIGHT/2);

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

                // move the card to the placing position:
                Tween tweenmovecard = GetTree().CreateTween();
                tweenmovecard.SetTrans(Tween.TransitionType.Linear);
                tweenmovecard.SetEase(Tween.EaseType.In);
                tweenmovecard.TweenProperty(card, "global_position", CardPlacingPosition.Position, 0.6);

                Tween tweenmovehand = GetTree().CreateTween();
                tweenmovehand.TweenProperty(this, "position", downPosition, 0.2);

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
