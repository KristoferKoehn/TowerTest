using Godot;
using System;
using System.Collections.Generic;

public partial class DeckManager : Node
{
    private static DeckManager instance;

    public List<CardData> cards;

    public static DeckManager GetInstance()
    {
        if (instance == null)
        {
            instance = new DeckManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "DeckManager";
        }
        return instance;
    }

    public void AddCard(CardData card)
    {
        cards.Add(card);
    }

    public void SetDeck(List<CardData> cards) {
        this.cards = cards;
    }

    public List<CardData> DrawCards(int count)
    {
        List<CardData> drawn = new List<CardData>();

        Random random = new Random();
        int idx = random.Next(cards.Count);
        drawn.Add(cards[idx]);
        cards.Remove(cards[idx]);
        return cards;
    }

}

