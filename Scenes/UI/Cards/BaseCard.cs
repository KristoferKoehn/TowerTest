using Godot;
using TowerTest.Scenes.Components;

public partial class BaseCard : Control
{
    [Signal]
    public delegate void SelectedEventHandler(BaseCard sender);
    [Signal]
    public delegate void CancelledEventHandler(BaseCard sender);
    [Signal]
    public delegate void PlacedEventHandler(BaseCard sender);
    [Signal]
    public delegate void DragStartedEventHandler(BaseCard sender);
    [Signal]
    public delegate void DragEndedEventHandler(BaseCard sender);

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
    public bool Disabled = false;

    AbstractPlaceable QueriedPlaceable { get; set; } = null;

    //turns of selectability. Used for shops and query focus
    public bool ActiveInHand = true;

    public bool Generated = true;
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
        if (Disabled) { Highlighted = false; return; }
        Highlighted = true;
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.TweenProperty(this, "scale", this.selectedScale, 0.2);
    }

    public void _on_mouse_exited()
    {
        if (Disabled) { Highlighted = false; return; }
        Highlighted = false;
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.TweenProperty(this, "scale", this.originalScale, 0.2);
    }

    public void _on_gui_input(InputEvent @event)
    {
        if (Disabled) { Highlighted = false; return; }
        if (@event is InputEventMouseButton mouseEvent)
        {

            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsReleased() && pressed)
            {
                pressed = false;
                DragOffset = Vector2.Zero;
                MotionAccumulation = Vector2.Zero;
                Active = true;
                dragging = false;
                EmitSignal("DragEnded", this);
                Tween tweenscale = GetTree().CreateTween();
                tweenscale.TweenProperty(this, "scale", originalScale, 0.2);
            }

            /*
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsReleased() && pressed)
            {
                pressed = false;
                DragOffset = Vector2.Zero;
                MotionAccumulation = Vector2.Zero;
                if (!dragging)
                {
                    if (Active)
                    {
                        if (PlayerStatsManager.GetInstance().GetStat(data.ResourceCostType) >= data.ResourceCostValue)
                        {
                            EmitSignal("Selected", this);
                        } else
                        {
                            //not enough resources, doesn't work
                            Tween t = GetTree().CreateTween();
                            t.TweenProperty(this, "global_position", GlobalPosition - new Vector2(0, -100), 0.1f);
                        }
                    }
                }

                Active = true;
                dragging = false;
                Tween tweenscale = GetTree().CreateTween();
                tweenscale.TweenProperty(this, "scale", this.originalScale, 0.2);
            }
            */


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
        if (MotionAccumulation.Length() > 4)
        {
            dragging = true;
            Active = false;
            EmitSignal("DragStarted", this);
        }

    }

    public override void _Input(InputEvent @event)
    {
        if (Disabled) { Highlighted = false; return; }

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
        GD.Print(data.Name);
        CardNameLabel.Text = data.Name;
        CardName = data.Name;
        if (Viewport != null)
        {
            Viewport.QueueFree();
        }
        Viewport = ViewportScene.Instantiate<ViewportVisuals>();
        Viewport.Size = (Vector2I)CardViewportFrame.Size;
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

    public void Play()
    {
        switch(this.data.CardType)
        {
            case CardType.Tower:
            case CardType.Chunk:
            case CardType.Spell:
                SpawnPlaceable();
                break;
            case CardType.Artifact:
                break;
        }
    }

    private void ActivateArtifact()
    {
        ArtifactManager.GetInstance().AddArtifact(CardLoadingManager.GetInstance().GetPackedScene(this.data.SubjectScene).Instantiate<BaseArtifact>());
    }

    private void SpawnPlaceable()
    {
        // Load and instantiate the card:
        AbstractPlaceable Placeable = CardLoadingManager.GetInstance().GetPackedScene(this.data.SubjectScene).Instantiate<AbstractPlaceable>();
        QueriedPlaceable = Placeable;
        QueriedPlaceable.Cancelled += CancelPlacement;
        QueriedPlaceable.Placed += SuccessfullyPlaced;

        SceneSwitcher.CurrentGameLoop.AddChild(Placeable);
        

        Placeable.ActivatePlacing();
    }

    public void SuccessfullyPlaced(Node3D item, Vector3 pos)
    {
        QueriedPlaceable.Cancelled -= CancelPlacement;
        QueriedPlaceable.Placed -= SuccessfullyPlaced;
        QueriedPlaceable = null;
        AccountStatsManager.GetInstance().ChangeStat(data.ResourceCostType, -data.ResourceCostValue);
        EmitSignal("Placed", this);
    }

    public void CancelPlacement()
    {
        if (QueriedPlaceable != null)
        {
            QueriedPlaceable = null;
        }
        EmitSignal("Cancelled", this);
    }

}
