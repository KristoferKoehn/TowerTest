using Godot;
using System;

public partial class DeckViewerPanel : Control
{
	[Export] HFlowContainer FlowContainer;
	[Export] Label SizeLabel;
	[Export] Label NumChunksLabel;
	[Export] Label NumTowersLabel;
	[Export] Label NumActionsLabel;

	PackedScene BaseCardScene = GD.Load<PackedScene>("res://Scenes/UI/Cards/BaseCard.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (CardData cardData in DeckManager.GetInstance().Cards)
		{
			BaseCard card = BaseCardScene.Instantiate<BaseCard>();
			card.SetCardData(cardData);
			TextureRect textRect = new TextureRect();
			textRect.Size = new Vector2(62.5f, 87.5f);
			textRect.AddChild(card);
			ViewportTexture texture = new ViewportTexture();
            textRect.Texture = texture;
            FlowContainer.AddChild(textRect);
		}
	}

    public void _on_button_pressed()
	{
		QueueFree();
	}
}
