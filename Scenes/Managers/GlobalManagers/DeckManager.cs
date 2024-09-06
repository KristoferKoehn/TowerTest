using Godot;
using System;
using System.Collections.Generic;

public partial class DeckManager : Node
{
    public event Action<CardData> CardAdded;
    public event Action<CardData> CardRemoved;

    private static DeckManager instance;

    public List<CardData> TotalCards = new();
    public List<CardData> DrawnCards = new();
    public List<CardData> Cards = new();
    public List<CardData> Discards = new();

    public static DeckManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new DeckManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "DeckManager";
        }
        return instance;
    }

    public void AddCard(CardData card)
    {
        Cards.Add(card);
        TotalCards.Add(card);

        OnCardAdded(card);
    }

    public void SetDeck(DeckData deck) {
        List<CardData> t = new();
        List<CardData> c = new();

        foreach(CardData card in deck.data) {
            t.Add(card);
            c.Add(card);
        }

        TotalCards = t;
        Cards = c;
        foreach (CardData card in Cards)
        {
            OnCardAdded(card);
        }
    }

    public List<CardData> DrawCards(int count)
    {

        List<CardData> draw = new List<CardData>();
        Random random = new Random();
        int idx;
        for (int i = 0; i < count; i++)
        {
            idx = random.Next(Cards.Count);
            draw.Add(Cards[idx]); 
            DrawnCards.Add(Cards[idx]);
            Cards.Remove(Cards[idx]);
        }
        return draw;
    }

    public void Discard(CardData card)
    {
        Discards.Add(card);
        if (Cards.Contains(card))
        {
            Cards.Remove(card);
        }

        OnCardRemoved(card);
    }

    public int RemainingCards()
    {
        return Cards.Count;
    }

    public void RefreshDeck()
    {
        Cards.AddRange(Discards);
    }

    protected virtual void OnCardAdded(CardData card)
    {
        CardAdded?.Invoke(card);
    }

    protected virtual void OnCardRemoved(CardData card)
    {
        CardRemoved?.Invoke(card);
    }
}

