using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Control
{

    private List<BaseCard> CardList = new List<BaseCard>();

    [Export]
    Control CardPlacingPosition;
    [Export]
    Path2D CardPlacingPath;
    [Export]
    PathFollow2D PathFollow;
    [Export]
    PackedScene BaseCardScene;

    List<float> CardPositions = new List<float>();

    public override void _Ready()
    {

        this.GenerateHandWithAllCards();
        UpdatePositions();
    }

    public override void _Process(double delta)
    {
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
        //UpdateCardPositions();
    }

    public void UpdatePositions()
    {
        CardPositions.Clear();
        float PathLength = CardPlacingPath.Curve.GetBakedLength();
       
        if (CardList.Count > 1)
        {
            for (int i = 0; i < CardList.Count; i++)
            {
                CardPositions.Add(i / (CardList.Count - 1.0f) * PathLength);
            }

        } else if (CardList.Count == 1)
        {
            CardPositions.Add(PathLength / 2f);
        } else
        {
            //do nothing?
        }


    }

    public void UpdateCardPositions()
    {
        if (CardList.Count <= 2)
        {
            //come up with some way to do 1 and 2 card hands
            return;
        }

        float threshold = 1.0f / (CardList.Count) * CardPlacingPath.Curve.GetBakedLength();

        for (int i = 0; i < CardList.Count; i++)
        {
            //position swapping the cards, even if dragging
            Vector2 CurvePoint = CardPlacingPath.Curve.GetClosestPoint(CardPlacingPath.ToLocal(CardList[i].GlobalPosition));
            float RealProgress = CardPlacingPath.Curve.GetClosestOffset(CurvePoint);

            if (RealProgress - (i / (CardList.Count - 1.0f) * CardPlacingPath.Curve.GetBakedLength()) > threshold && i != CardList.Count-1)
            {
                BaseCard temp = CardList[i + 1];
                CardList[i + 1] = CardList[i];
                CardList[i] = temp;
            } else if (RealProgress - (i / (CardList.Count - 1.0f) * CardPlacingPath.Curve.GetBakedLength()) < -threshold && i != 0)
            {
                BaseCard temp = CardList[i - 1];
                CardList[i - 1] = CardList[i];
                CardList[i] = temp;
            }

            if (CardList[i].Active)
            {
                PathFollow.Progress = CardPositions[i];
                Vector2 Placement = new Vector2(PathFollow.GlobalPosition.X, PathFollow.GlobalPosition.Y);
                Tween t = GetTree().CreateTween();
                
                t.TweenProperty(CardList[i], "global_position", Placement, 0.1f);
            }
        }
    }

    // When a chunk card is clicked:
    public void OnChunkCardClicked(InputEvent @event, BaseCard card)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
           
        }
    }

    // Fills the player hand with one of every chunk card possible.
    private void GenerateHandWithAllCards()
    {
        foreach(BaseCard bc in CardList)
        {
            bc.QueueFree();
        }
        this.CardList.Clear();

        foreach (CardData card in CardLoadingManager.GetInstance().GetAllCardData())
        {
            BaseCard chunkCard = BaseCardScene.Instantiate<BaseCard>();
            chunkCard.SetCardData(card);
            AddCard(chunkCard);
        }
    }
}
