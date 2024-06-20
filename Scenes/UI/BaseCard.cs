using Godot;
using System;
using System.Collections.Generic;

public partial class BaseCard : Control
{
    public string CardName { get; set; }
    public List<string> CardInfo { get; private set; }
    public string CardImgPath { get; private set; }
    public string ChunkPath { get; private set; }

    public PackedScene ViewportScene = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn");
    public ViewportVisuals Viewport;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void SetCard(string ChunkName)
    {
        CardName = ChunkName.ReplaceN(".tscn","");
        //CardImgPath = $"res://Assets/ChunkImages/{CardName}.png";
        ChunkPath = $"res://Scenes/Chunks/{CardName}.tscn";

        Viewport = ViewportScene.Instantiate<ViewportVisuals>();
        Viewport.OwnWorld3D = true;
        this.AddChild(Viewport);
        Viewport.CameraZoom = 12;
        Viewport.CameraOrbitSpeed = 0.2f;
        Viewport.Camera.Fov = 55;

        Viewport.SubjectPackedScene = GD.Load<PackedScene>(ChunkPath);

        // Set the ChunkName
        Label chunkNameLabel = GetNode<Label>("MarginContainer/Bars/TopBar/Name/CenterContainer/ChunkName");
        chunkNameLabel.Text = CardName;

        // Set the texture
        TextureRect textureRect = GetNode<TextureRect>("MarginContainer/Bars/Panel/TextureRect");
        textureRect.Texture = Viewport.GetTexture();
        //var texture = (Texture)ResourceLoader.Load(CardImgPath);
        //textureRect.Texture = (Texture2D)texture;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
