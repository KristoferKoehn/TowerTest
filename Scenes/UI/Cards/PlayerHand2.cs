using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerHand2 : Control
{


    [Export]
    Node2D CardPlacingPosition;
    [Export]
    Path2D CardPlacingPath;
    [Export]
    PathFollow2D PathFollow;
    [Export]
    PackedScene BaseCardScene;

    [Export] Panel PlayPanel;
    [Export] Panel DiscardPanel;
    [Export] Panel FreezePanel;

    List<BaseCard> CardList = new List<BaseCard>();
    List<float> CardPositions = new List<float>();

    List<BaseCard> FreezeCardList = new List<BaseCard>();


    BaseCard DraggingCard = null;

    BaseCard ActiveCard = null;
    bool CardActive = false;

    float SwapThreshold = 0;

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {

        if (CardList.Count == 0)
        {
            List<CardData> DrawnCards = DeckManager.GetInstance().DrawCards((int)PlayerStatsManager.GetInstance().GetStat(StatType.HandSize));
            GD.Print(DrawnCards.ToString());
            foreach (CardData card in DrawnCards)
            {
                BaseCard chunkCard = BaseCardScene.Instantiate<BaseCard>();
                chunkCard.SetCardData(card);
                AddCard(chunkCard);
            }
        }

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 screenSize = GetViewportRect().Size;

        if (mousePos.Y > (screenSize.Y - 230) && !Input.IsActionPressed("camera_drag") && DraggingCard == null) 
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(CardPlacingPath, "global_position", new Vector2(screenSize.X / 2f, screenSize.Y - 90), 0.2);
        } 
        else
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(CardPlacingPath, "global_position", new Vector2(screenSize.X/2f, screenSize.Y + 60), 0.1);
        }
        
        UpdateCardPositions();
    }

    public void AddCard(BaseCard card)
    {
        card.Selected += CardSelected;
        card.Cancelled += CardCancelled;
        card.Placed += CardPlaced;
        card.DragStarted += (BaseCard) =>
        {
            DraggingCard = BaseCard;
        };
        card.DragStarted += RevealDropPanels;
        card.DragEnded += HideDropPanels;
        card.DragEnded += CardDropCheck;
        card.DragEnded += (BaseCard) =>
        {
            DraggingCard = null;
        };
        AddChild(card);
        card.GlobalPosition = CardPlacingPosition.GlobalPosition;
        CardList.Add(card);
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
        t.Finished += PlayActiveCard;
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

    public void PlayActiveCard()
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

        Vector2 MiddleOfFrame = FreezePanel.GlobalPosition + new Vector2(125, 175);
        for (int i = 0; i < FreezeCardList.Count; i++)
        {
            if (!FreezeCardList[i].Active) continue;

            FreezeCardList[i].MoveToFront();
            Vector2 position = new Vector2(MiddleOfFrame.X + 30 * i, MiddleOfFrame.Y);
            Tween t = GetTree().CreateTween();
            t.TweenProperty(FreezeCardList[i], "global_position", position, 0.1f);

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

    public void CardDropCheck(BaseCard baseCard)
    {

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 PlayPanelEnd = PlayPanel.GlobalPosition + PlayPanel.Size;

        Vector2 FreezePanelEnd = FreezePanel.GlobalPosition + FreezePanel.Size;

        if (PlayPanel.GlobalPosition.X < mousePos.X && PlayPanelEnd.X > mousePos.X && PlayPanel.GlobalPosition.Y < mousePos.Y && PlayPanelEnd.Y > mousePos.Y) 
        {
            baseCard.EmitSignal("Selected", baseCard);
        } else if (FreezePanel.GlobalPosition.X < mousePos.X && FreezePanelEnd.X > mousePos.X && FreezePanel.GlobalPosition.Y < mousePos.Y && FreezePanelEnd.Y > mousePos.Y)
        {
            CardList.Remove(baseCard);
            FreezeCardList.Add(baseCard);
        } else 
        {
            if (FreezeCardList.Contains(baseCard))
            {
                FreezeCardList.Remove(baseCard);
                CardList.Add(baseCard);
            }
        }
    }

    public void HideDropPanels(BaseCard card)
    {
        Vector2 ScreenSize = GetViewportRect().Size;

        Tween PlayTween = GetTree().CreateTween();
        PlayTween.TweenProperty(PlayPanel, "global_position", new Vector2(ScreenSize.X * 0.152f, ScreenSize.Y * -0.725f), 0.1);

        Tween DiscardTween = GetTree().CreateTween();
        DiscardTween.TweenProperty(DiscardPanel, "global_position", new Vector2(ScreenSize.X * 1.092f, ScreenSize.Y * 0.555f), 0.1);

        if (FreezeCardList.Count > 0)
        {
            Tween FreezeTween = GetTree().CreateTween();
            FreezeTween.TweenProperty(FreezePanel, "global_position", new Vector2(-50f, ScreenSize.Y * 0.555f), 0.1);
        } else
        {
            Tween FreezeTween = GetTree().CreateTween();
            FreezeTween.TweenProperty(FreezePanel, "global_position", new Vector2(ScreenSize.X * -0.308f, ScreenSize.Y * 0.555f), 0.1);
        }

    }

    public void RevealDropPanels(BaseCard card)
    {
        Vector2 ScreenSize = GetViewportRect().Size;

        Tween PlayTween = GetTree().CreateTween();
        PlayTween.TweenProperty(PlayPanel, "global_position", new Vector2(ScreenSize.X * 0.152f, ScreenSize.Y * 0.125f), 0.1);

        Tween DiscardTween = GetTree().CreateTween();
        DiscardTween.TweenProperty(DiscardPanel, "global_position", new Vector2(ScreenSize.X * 0.892f, ScreenSize.Y * 0.555f), 0.1);

        Tween FreezeTween = GetTree().CreateTween();
        FreezeTween.TweenProperty(FreezePanel, "global_position", new Vector2(-125, ScreenSize.Y * 0.555f), 0.1);
    }
}
