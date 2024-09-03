using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class DeckViewerPanel : Control
{
    [Export] HFlowContainer FlowContainer;
    [Export] Label DeckSizeLabel;
    [Export] Label NumChunksLabel;
    [Export] Label NumTowersLabel;
    [Export] Label NumActionsLabel;
    [Export] HSlider CardSizeSlider;

    private int ChunkCount;
    private int TowerCount;
    private int ActionCount;
    private bool AscendingOrder = true;
    private bool ShowDuplicates = true;
    private int currentSortIndex = 0; // Start sorting by name
    private int currentFilterIndex = 0; // Default should display all


    PackedScene CardScene = GD.Load<PackedScene>("res://Scenes/UI/Cards/Card.tscn");

    private List<(TextureRect textureRect, Card card)> originalCardEntries = new List<(TextureRect, Card)>();
    private List<(TextureRect textureRect, Card card)> filteredCardEntries = new List<(TextureRect, Card)>();

    public override void _Ready()
    {
        foreach (CardData cardData in DeckManager.GetInstance().TotalCards)
        {
            TextureRect textRect = new TextureRect();
            textRect.CustomMinimumSize = new Vector2(62.5f, 87.5f);
            textRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

            Card card = CardScene.Instantiate<Card>();
            textRect.AddChild(card);
            card.SetCardData(cardData);
            textRect.Texture = card.Texture;
            card.Visible = false;

            originalCardEntries.Add((textRect, card));
            filteredCardEntries.Add((textRect, card));

            switch (card.data.CardType)
            {
                case CardType.Chunk:
                    ChunkCount++; break;
                case CardType.Tower:
                    TowerCount++; break;
                case CardType.Action:
                    ActionCount++; break;
            }

            if (!DeckManager.GetInstance().DrawnCards.Contains(cardData)) // Broken, contains doesnt distinguish duplicates
            {
                // Darken the card to show it's in the discard pile
                textRect.Modulate = new Color(0.5f, 0.5f, 0.5f, 1); // Adjust RGB to 0.5 for a darker appearance
            }
        }

        DeckSizeLabel.Text = "Total Cards: " + DeckManager.GetInstance().Cards.Count.ToString();
        NumChunksLabel.Text = "Chunks: " + ChunkCount;
        NumTowersLabel.Text = "Towers: " + TowerCount;
        NumActionsLabel.Text = "Actions: " + ActionCount;

        FilterCards();
        SortCards();
        RePopulateFlowContainer();
        CardSizeSlider.ValueChanged += OnCardSizeSliderValueChanged;
    }

    private void RePopulateFlowContainer()
    {
        foreach (Node child in FlowContainer.GetChildren())
        {
            FlowContainer.RemoveChild(child);
        }

        foreach (var entry in filteredCardEntries)
        {
            FlowContainer.AddChild(entry.textureRect);
        }
    }

    private void OnCardSizeSliderValueChanged(double value)
    {
        foreach (var entry in originalCardEntries)
        {
            float width = 62.5f + (float)value * 1.5f;
            float height = 87.5f + (float)value * 2.1f;
            entry.textureRect.CustomMinimumSize = new Vector2(width, height);
        }

        RePopulateFlowContainer();
    }

    public void _on_button_pressed()
    {
        QueueFree();
    }

    public void _on_filter_option_button_item_selected(int index)
    {
        currentFilterIndex = index;
        FilterCards();
        SortCards(); // because you're adding new cards to the filtered list, you have to make sure they are sorted into the current sort.
        RePopulateFlowContainer();
    }

    public void _on_sort_option_button_item_selected(int index)
    {
        currentSortIndex = index;
        SortCards();
        RePopulateFlowContainer();
    }

    public void _on_order_button_pressed()
    {
        AscendingOrder = !AscendingOrder;
        SortCards();
        RePopulateFlowContainer();
    }

    public void _on_show_duplicates_button_pressed()
    {
        ShowDuplicates = !ShowDuplicates;
        if (ShowDuplicates)
        {
            // Remove all duplicate count labels before showing duplicates
            foreach (var entry in originalCardEntries)
            {
                RemoveDuplicateLabels(entry.textureRect);
            }
        }

        FilterCards();
        _on_sort_option_button_item_selected(currentSortIndex);
    }

    private void FilterCards()
    {
        switch (currentFilterIndex)
        {
            case 0: // All
                filteredCardEntries = originalCardEntries;
                break;
            case 1: // only Chunks
                filteredCardEntries = originalCardEntries.Where(entry => entry.card.data.CardType == CardType.Chunk).ToList();
                break;
            case 2: // only Towers
                filteredCardEntries = originalCardEntries.Where(entry => entry.card.data.CardType == CardType.Tower).ToList();
                break;
            case 3: // only Actions
                filteredCardEntries = originalCardEntries.Where(entry => entry.card.data.CardType == CardType.Action).ToList();
                break;

            // These cases are Broken, contains doesnt distinguish duplicates:
            case 4: // only In Hand (the cards that are in drawn cards and not in discard yet)
                filteredCardEntries = originalCardEntries
                    .Where(entry => DeckManager.GetInstance().DrawnCards.Contains(entry.card.data)
                                 && !DeckManager.GetInstance().Discards.Contains(entry.card.data))
                    .ToList();
                break;
            case 5: // only Not In Hand (the cards that are either still yet to be drawn or have been discarded)
                filteredCardEntries = originalCardEntries
                    .Where(entry => (DeckManager.GetInstance().Cards.Contains(entry.card.data)
                                 || DeckManager.GetInstance().Discards.Contains(entry.card.data)))
                    .ToList();
                break;
        }

        if (!ShowDuplicates)
        {
            var groupedEntries = filteredCardEntries
                .GroupBy(entry => entry.card.data)
                .Select(group =>
                {
                    var firstEntry = group.First();
                    int count = group.Count();

                    if (count > 1)
                    {
                        AddDuplicateLabel(firstEntry.textureRect, count);
                    }

                    return firstEntry;
                })
                .ToList();

            filteredCardEntries = groupedEntries;
        }
    }

    private void SortCards()
    {
        // Apply sorting based on the current sort option and order
        switch (currentSortIndex)
        {
            case 0: // by Name
                filteredCardEntries = AscendingOrder
                    ? filteredCardEntries.OrderBy(entry => entry.card.data.Name).ToList()
                    : filteredCardEntries.OrderByDescending(entry => entry.card.data.Name).ToList();
                break;
            case 1: // by Rarity
                filteredCardEntries = AscendingOrder
                    ? filteredCardEntries.OrderBy(entry => entry.card.data.Rarity).ToList()
                    : filteredCardEntries.OrderByDescending(entry => entry.card.data.Rarity).ToList();
                break;
            case 2: // by Type
                filteredCardEntries = AscendingOrder
                    ? filteredCardEntries.OrderBy(entry => entry.card.data.CardType).ToList()
                    : filteredCardEntries.OrderByDescending(entry => entry.card.data.CardType).ToList();
                break;
        }
    }

    private void AddDuplicateLabel(TextureRect textureRect, int count)
    {
        Label duplicateCountLabel = new Label
        {
            Text = $"{count}x",
            Modulate = Colors.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(32, 32),
        };

        textureRect.AddChild(duplicateCountLabel);
    }

    private void RemoveDuplicateLabels(TextureRect textureRect)
    {
        foreach (Node child in textureRect.GetChildren())
        {
            if (child is Label label && label.Text.EndsWith("x"))
            {
                textureRect.RemoveChild(label);
                label.QueueFree();
            }
        }
    }

}
