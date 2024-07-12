using Godot;
using System;
using System.Collections.Generic;

public partial class BaseCard : Control
{
    public string CardName { get; set; }
    public List<string> CardInfo { get; private set; }
    public string CardImgPath { get; private set; }
    public string ScenePath { get; private set; }

    public PackedScene ViewportScene = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn");
    public ViewportVisuals Viewport;

    private Vector2 originalPosition;

    public void _on_mouse_entered()
    {
        this.originalPosition = this.Position;
        // Shift the card up when hovered over
        Tween tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            this,
            "position",
            new Vector2(this.Position.X, Position.Y - 10),  // Adjust the amount you want to shift the card
            0.2f
        );
    }

    public void _on_mouse_exited()
    {
        // Move the card back to its original position
        Tween tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            this,
            "position",
            new Vector2(this.Position.X, originalPosition.Y),
            0.2f
        );
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        originalPosition = Position;
    }

    // Sets the card to the given scene (ex: a chunk name or a tower name)
    public void SetCard(string sceneName)
    {
        CardName = sceneName.ReplaceN(".tscn","");
        string ScenePath = string.Empty;

        Viewport = ViewportScene.Instantiate<ViewportVisuals>();
        Viewport.OwnWorld3D = true;
        this.AddChild(Viewport);
        Viewport.CameraZoom = 12;
        Viewport.CameraOrbitSpeed = 0.2f;
        Viewport.Camera.Fov = 55;

        if (CardDatabase.chunkslist.Contains(CardName))
        {
            ScenePath = $"res://Scenes/Chunks/{CardName}.tscn";
        }
        if (CardDatabase.towerslist.Contains(CardName))
        {
            this.Viewport.CameraZoom = 2f;
            this.Viewport.CameraTilt = -5f;
            ScenePath = $"res://Scenes/Towers/{CardName}.tscn";
        }

        Viewport.SubjectPackedScene = GD.Load<PackedScene>(ScenePath);

        // Set the sceneName
        Label chunkNameLabel = GetNode<Label>("MarginContainer/Bars/TopBar/Name/CenterContainer/SceneName");
        chunkNameLabel.Text = CardName;

        // Set the texture
        TextureRect textureRect = GetNode<TextureRect>("MarginContainer/Bars/Panel/TextureRect");
        textureRect.Texture = Viewport.GetTexture();
        //var texture = (Texture)ResourceLoader.Load(CardImgPath);
        //textureRect.Texture = (Texture2D)texture;

        // Set the border based on the rarity
        List<string> cardinfo = CardDatabase.DATA[CardName];
        string rarity = cardinfo[1];
        SetCardRarityColor(rarity);
    }

    private void SetCardRarityColor(string rarity)
    {
        Panel CardBackground = GetNode<Panel>("MarginContainer/MarginContainer/Background");
        StyleBoxTexture newStyleBox;

        switch (rarity.ToLower())
        {
            case "common":
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/CommonRarityStyleBox.tres");
                break;
            case "uncommon":
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/UncommonRarityStyleBox.tres");
                break;
            case "rare":
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/RareRarityStyleBox.tres");
                break;
            case "epic":
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/EpicRarityStyleBox.tres");
                break;
            case "legendary":
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/LegendaryRarityStyleBox.tres");
                break;
            default: // default to common:
                newStyleBox = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/CommonRarityStyleBox.tres");
                break;
        }

        // Apply the StyleBoxTexture to the Panel
        CardBackground.AddThemeStyleboxOverride("panel", newStyleBox);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
