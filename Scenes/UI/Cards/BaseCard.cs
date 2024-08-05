using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;

public partial class BaseCard : Control
{

    public string CardName { get; set; }
    public string CardInfo { get; private set; }
    public string CardImgPath { get; private set; }
    public string ScenePath { get; private set; }

    [Export] public Label CardNameLabel { get; set; }
    [Export] TextureRect CardViewportFrame { get; set; }
    [Export] Panel CardBackground {  get; set; }

    public PackedScene ViewportScene = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn");
    public ViewportVisuals Viewport = null;

    private Vector2 originalPosition;
    private Color originalModulate;
    private Color selectedModulate = new Color(1, 1, 0.4f, 1);
    private Vector2 originalScale;
    private Vector2 selectedScale;

    /// <summary>
    /// primarily, the card needs to detect a press, and "activates" the subject scene for placing.
    /// abstract out the subject scene, we shouldn't need the database, or we'd be using something my dynamic.
    ///     -avoid passing dir and making decisions on that string
    /// investigate a better node structure. Need a way to scale it up and down gracefully. 
    /// </summary>


    public void _on_mouse_entered()
    {
        this.originalPosition = this.Position;
        // Shift the card up when hovered over
        /*
        Tween tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            this,
            "position",
            new Vector2(this.Position.X, Position.Y - 10),  // Adjust the amount you want to shift the card
            0.2f
        );
        */
        this.ZIndex +=1;
        // Optionally, add a slight scale up effect
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.SetTrans(Tween.TransitionType.Sine);
        tweenscale.SetEase(Tween.EaseType.Out);
        tweenscale.TweenProperty(this, "scale", this.selectedScale, 0.1);
        //this.Modulate = selectedModulate;
    }

    public void _on_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Check if the left mouse button is pressed
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
            }
        }
    }

    public void _on_mouse_exited()
    {
        // Move the card back to its original position
        /*
        Tween tween = GetTree().CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(
            this,
            "position",
            new Vector2(this.Position.X, originalPosition.Y),
            0.2f
        );
        */
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.SetTrans(Tween.TransitionType.Sine);
        tweenscale.SetEase(Tween.EaseType.Out);
        tweenscale.TweenProperty(this, "scale", this.originalScale, 0.1);
        this.ZIndex -= 1;
        this.Modulate = originalModulate;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        originalPosition = Position;
        originalModulate = Modulate;

        originalScale = Scale;
        selectedScale = Scale * 1.15f;
    }

    public void SetCardData(CardData data)
    {
        CardNameLabel.Text = data.Name;
        CardName = data.Name;
        if (Viewport != null)
        {
            Viewport.QueueFree();
        }
        Viewport = ViewportScene.Instantiate<ViewportVisuals>();
        Viewport.OwnWorld3D = true;
        this.AddChild(Viewport);
        Viewport.CameraZoom = data.CameraZoom;
        Viewport.CameraOrbitSpeed = data.CameraOrbitSpeed;
        Viewport.CameraTilt = data.CameraTilt;
        Viewport.Camera.Fov = data.FOV;
        if (data.SubjectScene != null)
        {
            Viewport.SetSubjectScene(CardLoadingManager.GetInstance().GetPackedScene(data.SubjectScene));
        }
        
        CardViewportFrame.Texture = Viewport.GetTexture();
        CardBackground.AddThemeStyleboxOverride("panel", CardLoadingManager.GetInstance().GetRarityTexture(data.Rarity));
    }

    private void SetCardRarityColor(string rarity)
    {
        // Apply the StyleBoxTexture to the Panel
        CardBackground.AddThemeStyleboxOverride("panel", CardLoadingManager.GetInstance().GetRarityTexture(rarity));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
