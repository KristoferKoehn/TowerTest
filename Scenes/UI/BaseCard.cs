using Godot;
using System;
using System.Collections.Generic;

public partial class BaseCard : Control
{
    public string CardName { get; private set; }
    public List<string> CardInfo { get; private set; }
    public string CardImgPath { get; private set; }
    public string ChunkPath { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CardName = "c_2EAngle"; // Change this to get a different card.
        CardInfo = CardDatabase.DATA[CardDatabase.Get(CardName)];
        CardImgPath = $"res://Assets/ChunkImages/{CardName}.png";
        ChunkPath = $"res://Scenes/Chunks/{CardName}.tscn";

        // Set card properties
        var CardBaseNode = this;

        // Set the ChunkName
        var chunkNameLabel = CardBaseNode.GetNode<Label>("MarginContainer/Bars/TopBar/Name/CenterContainer/ChunkName");
        chunkNameLabel.Text = CardName;

        // Set the texture
        var textureRect = CardBaseNode.GetNode<TextureRect>("MarginContainer/Bars/Panel/TextureRect");
        var texture = (Texture)ResourceLoader.Load(CardImgPath);
        textureRect.Texture = (Texture2D)texture;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
