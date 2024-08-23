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
        
    }

    public override void _Process(double delta)
    {

        /// when game starts, draw 7 cards.
        /// at the end of the round, put all non-frozen cards into discard
        /// then draw 7 more cards.
        /// if there is not enough cards in deck, shuffle discard into deck.
        /// these all need animations. they can be shitty.



        if (DeckManager.GetInstance().RemainingCards() < 1)
        {
            //reshuffle;
        }
        //do some animations when it draws and if it needs to reshuffle
        if (CardList.Count == 0)
        {
            List<CardData> DrawnCards = DeckManager.GetInstance().DrawCards((int)PlayerStatsManager.GetInstance().GetStat(StatType.HandSize));
            foreach (CardData card in DrawnCards)
            {
                BaseCard chunkCard = BaseCardScene.Instantiate<BaseCard>();
                chunkCard.SetCardData(card);
                AddCard(chunkCard);
            }
        }

        UpdateCardPositions();
    }

    public void AddCard(BaseCard card)
    {
        card.Selected += CardSelected;
        card.Cancelled += CardCancelled;
        card.Placed += CardPlaced;
        this.AddChild(card);
        card.GlobalPosition = CardPlacingPosition.GlobalPosition;
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
            ActiveCard.Play();
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
                //hack bullshit solution
                card.MoveToFront();
                PathFollow.Progress = CardPositions[idx];
                Vector2 Placement = new Vector2(PathFollow.GlobalPosition.X, PathFollow.GlobalPosition.Y ) - (card.Size / 2f * card.Scale);
                Tween t = GetTree().CreateTween();
                t.TweenProperty(card, "global_position", Placement, 0.1f);

            }
        }
    }

    public void DrawCards()
    {

        //get hand size from stat block
        //animate deck and cards to hand
        //on tween finish, add cards to hand.
        //if not enough cards, reshuffle
    }

    public void Reshuffle()
    {
        //animate discard deck and shuffle sound
        //on finish, put all discards back into deck
    }

}
