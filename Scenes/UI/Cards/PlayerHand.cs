using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand : Control
{


    [Export]
    Node2D CardPlacingPosition;
    [Export]
    Path2D CardPlacingPath;
    [Export]
    PathFollow2D PathFollow;
    [Export]
    PackedScene BaseCardScene;

    List<BaseCard> CardList = new List<BaseCard>();
    List<float> CardPositions = new List<float>();

    BaseCard ActiveCard = null;
    bool CardActive = false;

    float SwapThreshold = 0;

    public override void _Ready()
    {
        GenerateHandWithAllCards();
    }

    public override void _Process(double delta)
    {
        UpdateCardPositions();
    }

    public void AddCard(BaseCard card)
    {
        card.Selected += CardSelected;
        card.Cancelled += CardCancelled;
        card.Placed += CardPlaced;
        this.AddChild(card);
        this.CardList.Add(card);
    }

    public void CardSelected(BaseCard card)
    {
        if (CardActive)
        {
            ActiveCard.CancelPlacement();
            CardList.Add(ActiveCard);
        }

        Tween handTween = GetTree().CreateTween();
        handTween.TweenProperty(CardPlacingPath, "position", new Vector2(0, 1200), 0.2f);

        CardList.Remove(card);
        ActiveCard = card;
        CardActive = true;
        Tween t = GetTree().CreateTween();
        t.TweenProperty(card, "global_position", CardPlacingPosition.GlobalPosition, 0.2f);
        t.Finished += SpawnPlaceableFromActiveCard;
    }

    public void CardPlaced(BaseCard card)
    {
        Tween handTween = GetTree().CreateTween();
        handTween.TweenProperty(CardPlacingPath, "position", Vector2.Zero, 0.2f);
        handTween.Finished += () =>
        {
            ActiveCard = null;
            CardActive = false;
            card.QueueFree();
        };
    }

    public void CardCancelled(BaseCard card)
    {
        Tween handTween = GetTree().CreateTween();
        handTween.TweenProperty(CardPlacingPath, "position", Vector2.Zero, 0.2f);
        handTween.Finished += () =>
        {
            CardList.Add(card);
            ActiveCard = null;
            CardActive = false;
        };
    }

    public void SpawnPlaceableFromActiveCard()
    {
        if (CardActive)
        {
            ActiveCard.SpawnPlaceable();
        }
    }

    public void UpdatePositions()
    {

        CardPositions.Clear();
        float PathLength = CardPlacingPath.Curve.GetBakedLength();
       
        if (CardList.Count > 3)
        {
            for (int i = 0; i < CardList.Count; i++)
            {
                CardPositions.Add((i+1) / (CardList.Count + 1.0f) * PathLength);
            }
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / CardList.Count + 20;

        } else if (CardList.Count == 1)
        {

            CardPositions.Add(PathLength / 2f);
        } else if (CardList.Count == 2)
        {
            CardPositions.Add(PathLength / 3f);
            CardPositions.Add( 2 * PathLength / 3f);
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / 3 + 10;
        } else if (CardList.Count == 3)
        {
            CardPositions.Add(    PathLength / 4);
            CardPositions.Add(2 * PathLength / 4);
            CardPositions.Add(3 * PathLength / 4);
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / 4 + 10;
        }


    }

    public void UpdateCardPositions()
    {
        UpdatePositions();

        if (CardList.Count > 1)
        {
            for (int i = 0; i < CardList.Count; i++)
            {

                if (CardList[i].Active) continue;

                //position swapping the cards, even if dragging
                Vector2 CurvePoint = CardPlacingPath.Curve.GetClosestPoint(CardPlacingPath.ToLocal(CardList[i].GlobalPosition + (CardList[i].Size / 2 * CardList[i].Scale)));
                float RealProgress = CardPlacingPath.Curve.GetClosestOffset(CurvePoint);


                if (RealProgress - CardPositions[i] > Mathf.Abs(SwapThreshold) && CardList.Count == 2)
                {
                    BaseCard temp = CardList[1];
                    CardList[1] = CardList[0];
                    CardList[0] = temp;

                }
                else if (RealProgress - CardPositions[i] > SwapThreshold && i != CardList.Count)
                {
                    BaseCard temp = CardList[i + 1];
                    CardList[i + 1] = CardList[i];
                    CardList[i] = temp;
                    GD.Print($"swapped up {CardList[i].CardName} with {CardList[i + 1].CardName}");
                }
                else if (RealProgress - CardPositions[i] < -SwapThreshold && i != 0)
                {
                    BaseCard temp = CardList[i - 1];
                    CardList[i - 1] = CardList[i];
                    CardList[i] = temp;
                }
            }
        }

        foreach (BaseCard card in CardList)
        {
            if (card.Active)
            {
                int idx = CardList.IndexOf(card);
                PathFollow.Progress = CardPositions[idx];
                Vector2 Placement = new Vector2(PathFollow.GlobalPosition.X, PathFollow.GlobalPosition.Y ) - (card.Size / 2f * card.Scale);
                Tween t = GetTree().CreateTween();
                t.TweenProperty(card, "global_position", Placement, 0.1f);
            }
        }
    }


    bool hidden = false;
    public void ToggleHide()
    {
        if (hidden)
        {
            hidden = false;
            Tween handTween = GetTree().CreateTween();
            handTween.TweenProperty(CardPlacingPath, "position", Vector2.Zero, 0.2f);

        } else
        {
            hidden = true;
            Tween handTween = GetTree().CreateTween();
            handTween.TweenProperty(CardPlacingPath, "position", new Vector2(0, 1200), 0.2f);
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
