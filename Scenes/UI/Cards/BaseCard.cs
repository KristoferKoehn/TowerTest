using Godot;
using Godot.NativeInterop;
using System;
using System.Collections.Generic;
using TowerTest.Scenes.Components;

public partial class BaseCard : Control
{
    [Signal]
    public delegate void SelectedEventHandler(BaseCard sender);
    [Signal]
    public delegate void CancelledEventHandler(BaseCard sender);
    [Signal]
    public delegate void PlacedEventHandler(BaseCard sender);

    public string CardName { get; set; }
    public string CardInfo { get; private set; }

    [Export] public Label CardNameLabel { get; set; }
    [Export] TextureRect CardViewportFrame { get; set; }
    [Export] Panel CardBackground {  get; set; }

    public CardData data { get; set; }

    public PackedScene ViewportScene = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn");
    public ViewportVisuals Viewport = null;

    private Vector2 originalScale;
    private Vector2 selectedScale;


    //dragging detection
    bool Highlighted = false;
    Vector2 MotionAccumulation = Vector2.Zero;
    Vector2 DragOffset = Vector2.Zero;
    bool dragging = false;
    bool pressed = false;
    public bool Active = true;


    AbstractPlaceable QueriedPlaceable { get; set; } = null;

    //turns of selectability. Used for shops and query focus
    public bool ActiveInHand = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        originalScale = Scale;
        selectedScale = Scale * 1.15f;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void _on_mouse_entered()
    {
        this.ZIndex +=1;
        Highlighted = true;
        // Optionally, add a slight scale up effect
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.SetTrans(Tween.TransitionType.Sine);
        tweenscale.SetEase(Tween.EaseType.Out);
        tweenscale.TweenProperty(this, "scale", this.selectedScale, 0.1);
    }

    public void _on_mouse_exited()
    {
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.SetTrans(Tween.TransitionType.Sine);
        tweenscale.SetEase(Tween.EaseType.Out);
        tweenscale.TweenProperty(this, "scale", this.originalScale, 0.1);
        this.ZIndex -= 1;
        Highlighted = false;
    }


    public void _on_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsReleased())
            {
                pressed = false;
                DragOffset = Vector2.Zero;
                MotionAccumulation = Vector2.Zero;
                // Check if the left mouse button is pressed
                if (!dragging)
                {
                    if (Active)
                    {
                        EmitSignal("Selected");
                    }
                }
                Active = true;
                dragging = false;
            }

            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed())
            {
                pressed = true;
                DragOffset = GlobalPosition - GetGlobalMousePosition();
            }

        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (pressed)
            {
                MotionAccumulation += mouseMotion.Relative;
            }

            if (dragging)
            {
                Tween t = GetTree().CreateTween();
                t.TweenProperty(this, "global_position", GetGlobalMousePosition() + DragOffset, 0.1f);
            }
        }

        //mouse motion click/drag detection threshold
        if (MotionAccumulation.Length() > 10)
        {
            dragging = true;
            Active = false;
        }

    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsReleased())
            {
                if (dragging && !Highlighted)
                {
                    pressed = false;
                    DragOffset = Vector2.Zero;
                    MotionAccumulation = Vector2.Zero;
                    dragging = false;
                    Active = true;
                }
            }
        }
    }

    public void SetCardData(CardData data)
    {
        this.data = data;
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

    public void SpawnPlaceable()
    {
        // Load and instantiate the card:
        AbstractPlaceable Placeable = CardLoadingManager.GetInstance().GetPackedScene(this.data.SubjectScene).Instantiate<AbstractPlaceable>();
        QueriedPlaceable = Placeable;
        QueriedPlaceable.Cancelled += CancelPlacement;
        QueriedPlaceable.Placed += SuccessfullyPlaced;
        SceneSwitcher.root.AddChild(Placeable);
        Placeable.ActivatePlacing();
    }

    public void CancelPlacement(Node3D item)
    {
        QueriedPlaceable.Cancelled -= CancelPlacement;
        QueriedPlaceable.Placed -= SuccessfullyPlaced;
        if(!QueriedPlaceable.IsQueuedForDeletion())
        {
            QueriedPlaceable.QueueFree();
        }
        QueriedPlaceable = null;
        EmitSignal("Cancelled", this);
    }

    public void SuccessfullyPlaced(Node3D item, Vector3 pos)
    {
        QueriedPlaceable.Cancelled -= CancelPlacement;
        QueriedPlaceable.Placed -= SuccessfullyPlaced;
        QueriedPlaceable = null;
        EmitSignal("Placed", this);
    }

}
