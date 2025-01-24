using Godot;
using Managers;
using System.Collections.Generic;

public partial class PlayerHand2 : Control
{

    [Export] Node2D CardPlacingPosition;
    [Export] Path2D CardPlacingPath;
    [Export] PathFollow2D PathFollow;
    [Export] PackedScene CardScene = GD.Load<PackedScene>("res://Scenes/UI/Cards/Card.tscn");

    [Export] Node2D Discard;
    [Export] Node2D Deck;

    [Export] Panel PlayPanel;
    [Export] Panel DiscardPanel;
    [Export] Panel FreezePanel;
    [Export] PackedScene DummyCard;
    [Export] AudioStreamPlayer2D CardSound;
    [Export] AudioStreamPlayer2D TowerPlacedSound;
    [Export] AudioStreamPlayer2D PlacingSound;

    [Export] Button HandDisplayButton;

    [Export] Node2D RightButtonParent;
    [Export] Node2D LeftButtonParent;

    public List<Card> CardList = new List<Card>();
    List<float> CardPositions = new List<float>();

    public List<Card> FreezeCardList = new List<Card>();

    Card DraggingCard = null;

    Card ActiveCard = null;
    bool CardActive = false;

    float SwapThreshold = 0;
    public bool HandUp = true;

    bool PlacingCardAnimationLock = false;
    public override void _Ready()
    {
        this.ZIndex = -100; // ensures its behind everything like pause menus and such
        HideDeck();
        HideDiscard();
        DrawCards(7);

        WaveManager.GetInstance().WaveEnded += () => EndOfWaveMechanics();

    }

    public override void _Process(double delta)
    {

        Vector2 screenSize = GetViewportRect().Size;


        if (HandUp) 
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(CardPlacingPath, "global_position", new Vector2(screenSize.X / 2f, screenSize.Y - 90), 0.1);
            HandDisplayButton.Text = "HIDE HAND";
        } 
        else
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(CardPlacingPath, "global_position", new Vector2(screenSize.X/2f, screenSize.Y + 130), 0.1);
            HandDisplayButton.Text = "SHOW HAND";

        }

        UpdateCardPositions();
    }

    public Timer DrawCards(int count)
    {
        HandUp = true;
        //spread this out over time using timers.
        //shen the deck is fully shown, start timer. Whenever timer times out do the thing.

        ShowDeck().Finished += () =>
        {

            if (!IsInstanceValid(this)) return;
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

                    if (i < remainingCount)
                    {
                        CardSound.Play();
                        CardData DrawnCard = DeckManager.GetInstance().DrawCards(1)[0];
                        Card Card = CardScene.Instantiate<Card>();
                        Card.SetCardData(DrawnCard);
                        AddCard(Card);
                        i++;

                        if (DeckManager.GetInstance().Cards.Count == 0)
                        {
                            Deck.Visible = false;
                        }
                    }
                    else
                    {
                        ShowDiscard().Finished += () => RefreshDeck().Timeout += () => 
                        { 
                            DrawCards(afterShuffleCount); 
                            HideDiscard();
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
                        CardSound.Play();

                        CardData DrawnCard = DeckManager.GetInstance().DrawCards(1)[0];
                        Card Card = CardScene.Instantiate<Card>();
                        Card.SetCardData(DrawnCard);
                        AddCard(Card);
                        i++;
                    } else
                    {
                        HideDeck();
                        t.QueueFree();
                    }
                };
            }


        };

        Timer T = new Timer();
        AddChild(T);
        T.Start(0.15 * count);
        return T;

    }

    public void AddCard(Card card)
    {

        card.Selected += CardSelected;
        card.Cancelled += CardCancelled;
        card.Placed += CardPlaced;

        card.Cancelled += (Card) => { ShowButtons(); };

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

    public void CardSelected(Card card)
    {
        HandUp = false;
        HandDisplayButton.Disabled = true;
        PlacingSound.Play();

        if (PlacingCardAnimationLock)
        {
            return;
        }

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
        PlacingCardAnimationLock = true;
        t.TweenProperty(card, "global_position", CardPlacingPosition.GlobalPosition, 0.2f);
        t.Finished += () =>
        {
            PlacingCardAnimationLock = false;
            PlayActiveCard();
        };
    }

    public void CardPlaced(Card card)
    {
        TowerPlacedSound.Play();
        HandDisplayButton.Disabled = false;
        HandUp = true;


        ShowDiscard().Finished += () =>
        {
            Tween t = GetTree().CreateTween();
            t.TweenProperty(card, "position", Discard.Position, 0.2f);
            t.Finished += () =>
            {
                CardSound.Play();
                Discard.Visible = true;
                card.Discard();
                GetTree().CreateTimer(0.1).Timeout += () =>
                {
                    ShowButtons();
                    HideDiscard();
                };
            };
            ActiveCard = null;
            CardActive = false;
            
        };
    }

    public void CardCancelled(Card card)
    {
        HandDisplayButton.Disabled = false;
        HandUp = true;
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
                CardPositions.Add((i+2) / (CardList.Count + 3.0f) * PathLength);
            }
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / CardList.Count + 10;

        } else if (CardList.Count == 1)
        {

            CardPositions.Add(PathLength / 2f);
        } else if (CardList.Count == 2)
        {
            CardPositions.Add(PathLength / 3f);
            CardPositions.Add( 2 * PathLength / 3f);
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / 3 + 5;
        } else if (CardList.Count == 3)
        {
            CardPositions.Add(    PathLength / 4);
            CardPositions.Add(2 * PathLength / 4);
            CardPositions.Add(3 * PathLength / 4);
            SwapThreshold = CardPlacingPath.Curve.GetBakedLength() / 4 + 5;
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
                Vector2 CurvePoint = CardPlacingPath.Curve.GetClosestPoint(CardPlacingPath.ToLocal(CardList[i].GlobalPosition));
                float RealProgress = CardPlacingPath.Curve.GetClosestOffset(CurvePoint);

                if (RealProgress - CardPositions[i] > Mathf.Abs(SwapThreshold) && CardList.Count == 2)
                {
                    Card temp = CardList[1];
                    CardList[1] = CardList[0];
                    CardList[0] = temp;

                }
                else if (RealProgress - CardPositions[i] > SwapThreshold && i != CardList.Count)
                {
                    Card temp = CardList[i + 1];
                    CardList[i + 1] = CardList[i];
                    CardList[i] = temp;
                }
                else if (RealProgress - CardPositions[i] < -SwapThreshold && i != 0)
                {
                    Card temp = CardList[i - 1];
                    CardList[i - 1] = CardList[i];
                    CardList[i] = temp;
                }
            }
        }

        foreach (Card card in CardList)
        {
            if (card.Active)
            {
                
                int idx = CardList.IndexOf(card);
                //hack bullshit solution
                card.MoveToFront();

                if (!card.Highlighted)
                {
                    card.ZIndex = idx;
                }

                PathFollow.Progress = CardPositions[idx];
                Vector2 Placement = new Vector2(PathFollow.GlobalPosition.X, PathFollow.GlobalPosition.Y );
                Tween t = GetTree().CreateTween();
                t.TweenProperty(card, "global_position", Placement, 0.1f);

            }
        }

        Vector2 MiddleOfFrame = FreezePanel.GlobalPosition;
        for (int i = 0; i < FreezeCardList.Count; i++)
        {
            if (!FreezeCardList[i].Active) continue;

            FreezeCardList[i].MoveToFront();
            Vector2 position = new Vector2(MiddleOfFrame.X + 30 * i, MiddleOfFrame.Y);
            Tween t = GetTree().CreateTween();
            t.TweenProperty(FreezeCardList[i], "global_position", position, 0.1f);

        }
    }

    public void CardDropCheck(Card baseCard)
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
            ShowButtons();
        }
    }

    //hide buttons when drop panels are shown
    public void HideButtons()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(RightButtonParent, "position", new Vector2(400, 0), 0.2);

        Tween f = GetTree().CreateTween();
        f.TweenProperty(LeftButtonParent, "position", new Vector2(-400, 0), 0.2);
    }

    //Show buttons *only when card is no longer active*
    public void ShowButtons()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(RightButtonParent, "position", new Vector2(0, 0), 0.2);

        Tween f = GetTree().CreateTween();
        f.TweenProperty(LeftButtonParent, "position", new Vector2(0, 0), 0.2);
    }

    public void HideDropPanels(Card card)
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

    public void RevealDropPanels(Card card)
    {
        Vector2 ScreenSize = GetViewportRect().Size;

        Tween PlayTween = GetTree().CreateTween();
        PlayTween.TweenProperty(PlayPanel, "global_position", new Vector2(ScreenSize.X * 0.152f, ScreenSize.Y * 0.125f), 0.1);

        Tween DiscardTween = GetTree().CreateTween();
        DiscardTween.TweenProperty(DiscardPanel, "global_position", new Vector2(ScreenSize.X * 0.892f, ScreenSize.Y * 0.555f), 0.1);

        Tween FreezeTween = GetTree().CreateTween();
        FreezeTween.TweenProperty(FreezePanel, "global_position", new Vector2(-125, ScreenSize.Y * 0.555f), 0.1);

        HideButtons();
    }

    public Tween ShowDiscard()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Discard, "position", new Vector2(400, -125), 0.4).SetEase(Tween.EaseType.InOut);
        return t;
    }


    public Tween HideDiscard()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Discard, "position", new Vector2(1300, -125), 0.2);
        return t;
    }

    public Tween HideDeck()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Deck, "position", new Vector2(1300, -325), 0.2);
        return t;
    }

    public Tween ShowDeck()
    {
        Tween t = GetTree().CreateTween();
        t.TweenProperty(Deck, "position", new Vector2(400, -325), 0.7).SetEase(Tween.EaseType.InOut);
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
                CardSound.Play();
                Sprite2D dummy = DummyCard.Instantiate<Sprite2D>();
                AddChild(dummy);
                dummy.GlobalPosition = Discard.GlobalPosition;
                Tween t = GetTree().CreateTween();
                t.TweenProperty(dummy, "global_position", Deck.GlobalPosition, 0.2);
                t.Finished += () => {
                    CardSound.Play();
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
        cardSpawnTimer.Start(0.7f/maxCount);
        Timer timer = new Timer();
        this.AddChild(timer);
        timer.Start(0.7);
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
            foreach (Card card in CardList)
            {
                t = GetTree().CreateTween();
                t.TweenProperty(card, "global_position", Discard.GlobalPosition, 0.2).SetDelay(j * 0.2);
                t.Finished += () =>
                {
                    CardSound.Play();
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
        HandUp = true;
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

    public void _on_hand_display_button_pressed()
    {
        HandUp = !HandUp;
    }

    public void ForceHandUp(bool disabled)
    {
        HandUp = true;
        HandDisplayButton.Disabled = disabled;
    }

    public void ForceHandDown(bool disabled)
    {
        HandUp = false;
        HandDisplayButton.Disabled = disabled;
    }
}
