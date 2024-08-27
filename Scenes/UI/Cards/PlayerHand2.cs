using Godot;
using Managers;
using System;
using System.Collections.Generic;

public partial class PlayerHand2 : Control
{

    [Export] Node2D CardPlacingPosition;
    [Export] Path2D CardPlacingPath;
    [Export] PathFollow2D PathFollow;
    [Export] PackedScene BaseCardScene;

    [Export] Node2D Discard;
    [Export] Node2D Deck;

    [Export] Panel PlayPanel;
    [Export] Panel DiscardPanel;
    [Export] Panel FreezePanel;
    [Export] PackedScene DummyCard;

    public List<BaseCard> CardList = new List<BaseCard>();
    List<float> CardPositions = new List<float>();

    public List<BaseCard> FreezeCardList = new List<BaseCard>();

    BaseCard DraggingCard = null;

    BaseCard ActiveCard = null;
    bool CardActive = false;

    float SwapThreshold = 0;
    bool ForceHandUp = false;


    public override void _Ready()
    {
        HideDeck();
        HideDiscard();
        DrawCards(7);

        WaveManager.GetInstance().WaveEnded += () => EndOfWaveMechanics();

    }

    public override void _Process(double delta)
    {

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 screenSize = GetViewportRect().Size;

        if ((mousePos.Y > (screenSize.Y - 230) && !Input.IsActionPressed("camera_drag") && DraggingCard == null) || ForceHandUp) 
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

    public Timer DrawCards(int count)
    {
        ForceHandUp = true;
        //spread this out over time using timers.
        //shen the deck is fully shown, start timer. Whenever timer times out do the thing.

        ShowDeck().Finished += () =>
        {
            if (count > DeckManager.GetInstance().Cards.Count)
            {

                //if remaining cards is less than count,
                //draw remaining cards, save number, make Deck.Visible = false;
                //reshuffle deck
                //draw rest of cards (remaining - count)

                int remainingCount = DeckManager.GetInstance().Cards.Count;
                int afterShuffleCount = count - remainingCount;
                int i = 0;
                Timer t = new Timer();
                AddChild(t);
                t.Start(0.1);
                t.Timeout += () =>
                {
                    if (DeckManager.GetInstance().Cards.Count == 0)
                    {
                        Deck.Visible = false;
                    }

                    if (i < remainingCount)
                    {
                        CardData DrawnCard = DeckManager.GetInstance().DrawCards(1)[0];
                        BaseCard Card = BaseCardScene.Instantiate<BaseCard>();
                        Card.SetCardData(DrawnCard);
                        AddCard(Card);
                        i++;
                    }
                    else
                    {
                        ShowDiscard().Finished += () => RefreshDeck().Timeout += () => 
                        { 
                            HideDiscard();
                            DrawCards(afterShuffleCount); 
                        };
                        t.QueueFree();
                    }
                };


            } else
            {
                int i = 0;
                Timer t = new Timer();
                AddChild(t);
                t.Start(0.1);
                t.Timeout += () =>
                {
                    if (i < count)
                    {
                        CardData DrawnCard = DeckManager.GetInstance().DrawCards(1)[0];
                        BaseCard Card = BaseCardScene.Instantiate<BaseCard>();
                        Card.SetCardData(DrawnCard);
                        AddCard(Card);
                        i++;
                    } else
                    {
                        HideDeck();
                        t.QueueFree();
                        ForceHandUp = false;
                    }
                };
            }


        };

        Timer T = new Timer();
        AddChild(T);
        T.Start(0.15 * count);
        return T;

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
        card.Disabled = true;
        Tween t = GetTree().CreateTween();
        t.TweenProperty(card, "global_position", CardPlacingPosition.GlobalPosition, 0.2f);
        t.Finished += PlayActiveCard;
    }

    public void CardPlaced(BaseCard card)
    {
        
        ShowDiscard().Finished += () =>
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(card, "position", Discard.Position, 0.2f);
            t.Finished += () =>
            {
                Discard.Visible = true;
                card.Discard();
                GetTree().CreateTimer(0.1).Timeout += () => HideDiscard();
            };
            ActiveCard = null;
            CardActive = false;
            
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
            card.Disabled = false;
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

    public Tween ShowDiscard()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Discard, "position", new Vector2(400, -200), 0.4).SetEase(Tween.EaseType.InOut);
        return t;
    }


    public Tween HideDiscard()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Discard, "position", new Vector2(1000, -200), 0.2);
        return t;
    }

    public Tween HideDeck()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Deck, "position", new Vector2(1000, -400), 0.2);
        return t;
    }

    public Tween ShowDeck()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Deck, "position", new Vector2(400, -400), 0.7).SetEase(Tween.EaseType.InOut);
        return t;
    }
    
    public Timer RefreshDeck()
    {
        int maxCount = DeckManager.GetInstance().Discards.Count;
        int i = 0;
        Timer cardSpawnTimer = new Timer();
        AddChild(cardSpawnTimer);
        cardSpawnTimer.Timeout += () => {

            if (i < maxCount)
            {
                Sprite2D dummy = DummyCard.Instantiate<Sprite2D>();
                AddChild(dummy);
                dummy.GlobalPosition = Discard.GlobalPosition + new Vector2(62.5f, 175f / 2f);
                Tween t = GetTree().CreateTween();
                t.TweenProperty(dummy, "global_position", Deck.GlobalPosition + new Vector2(62.5f, 175f/2f), 0.2);
                t.Finished += () => {
                    Deck.Visible = true;
                    dummy.QueueFree(); 
                };
                i++;

                //turn off deck when no cards left
                if (i == maxCount)
                {
                    Discard.Visible = false;
                }
            } else
            {
                cardSpawnTimer.QueueFree();
            }
        };

        DeckManager.GetInstance().RefreshDeck();
        cardSpawnTimer.Start(0.1);
        Timer timer = new Timer();
        this.AddChild(timer);
        timer.Start(0.1 * maxCount);
        timer.Timeout += () =>
        {
            timer.QueueFree();
            Deck.Visible = true;
        };
        return timer;
    }

    public Tween DiscardHand()
    {

        Tween t = null;
        if (CardList.Count > 0)
        {
            int j = 0;
            foreach (BaseCard card in CardList)
            {
                t = GetTree().CreateTween();
                t.TweenProperty(card, "global_position", Discard.GlobalPosition, 0.2).SetDelay(j * 0.2);
                t.Finished += () =>
                {
                    Discard.Visible = true;
                    card.Discard();
                };
                j++;
            }
            CardList.Clear();

            return t;
        }

        t = GetTree().CreateTween();
        t.TweenProperty(this, "position", this.Position, 0);
        return t;
    }

    public void EndOfWaveMechanics()
    {

        if (CardList.Count > 0)
        {
            ShowDiscard().Finished += () => {
                DiscardHand().Finished += () => {
                    HideDiscard();
                    DrawCards(7);
                };
            };
        } else
        {
            DrawCards(7);
        }
    }
}
