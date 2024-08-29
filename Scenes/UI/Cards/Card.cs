using Godot;
using TowerTest.Scenes.Components;

public partial class Card : Sprite2D
{
    private bool Debugging = false;

    [Signal]
    public delegate void SelectedEventHandler(Card sender);
    [Signal]
    public delegate void CancelledEventHandler(Card sender);
    [Signal]
    public delegate void PlacedEventHandler(Card sender);
    [Signal]
    public delegate void DragStartedEventHandler(Card sender);
    [Signal]
    public delegate void DragEndedEventHandler(Card sender);

    public string CardName { get; set; }
    public string CardInfo { get; private set; }

    [Export] public Label CardNameLabel { get; set; }
    [Export] TextureRect CardViewportFrame { get; set; }
    [Export] Panel CardBackground {  get; set; }
    [Export] Viewport CardViewport { get; set; }
    public CardData data { get; set; }

    public PackedScene ViewportScene = GD.Load<PackedScene>("res://Scenes/Utility/ViewportVisuals.tscn");
    public ViewportVisuals Viewport = null;

    private Vector2 originalScale;
    private Vector2 selectedScale;

    //dragging detection
    public bool Highlighted = false;
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
        //when a new thing is selected, deselect old thing. Or something. Gotta pull the thing forward or some shit. Idk
        if(IsInGroup("selected") && GetTree().GetNodesInGroup("selected").Count < 2)
        {
            if (!Highlighted)
            {
                ZIndex += 2;
                GD.Print(ZIndex);
            }
            Highlighted = true;
            Tween tweenscale = GetTree().CreateTween();
            tweenscale.TweenProperty(this, "scale", this.selectedScale, 0.2);
        }


        //have to emulate UI tracking of inputs. if lmb is unpressed, make everything go back to normal
        //when clicked, make card follow mouse. if unclick detected AT ALL, make determination on whether or not dragged or clicked.
        //if clicked, do something different
        //if dragged, do nothing (hand handles this case)
        if(Input.IsActionJustReleased("select") && pressed)
        {
            pressed = false;
            DragOffset = Vector2.Zero;
            MotionAccumulation = Vector2.Zero;
            Active = true;
            dragging = false;
            EmitSignal("DragEnded", this);
            Tween tweenscale = GetTree().CreateTween();
            tweenscale.TweenProperty(this, "scale", originalScale, 0.2);

            if (MotionAccumulation.Length() < 1)
            {
                GD.Print("clicked");
            }
        }


        if (dragging)
        {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "global_position", GetGlobalMousePosition() + DragOffset, 0.1);

        }

    }


    public void _on_card_base_mouse_entered()
    {
        AddToGroup("selected");

        if (IsInGroup("selected") && GetTree().GetNodesInGroup("selected").Count < 2)
        {
            ZIndex += 2;
            if (Disabled) { Highlighted = false; return; }
            Highlighted = true;
            Tween tweenscale = GetTree().CreateTween();
            tweenscale.TweenProperty(this, "scale", this.selectedScale, 0.2);
        }
    }

    public void _on_card_base_mouse_exited()
    {

        ZIndex -= 2;
        RemoveFromGroup("selected");
        if (Disabled) { Highlighted = false; return; }
        Highlighted = false;
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.TweenProperty(this, "scale", this.originalScale, 0.2);

    }

    public void _on_card_base_gui_input(Node n, InputEvent @event, int idx)
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

                if (MotionAccumulation.Length() < 1)
                {
                    GD.Print("clicked");
                }

            }


            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.IsPressed() && Highlighted)
            {
                pressed = true;
                dragging = true;
                Active = false;
                DragOffset = GlobalPosition - GetGlobalMousePosition();
                EmitSignal("DragStarted", this);
            }
        }

        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (pressed)
            {
                MotionAccumulation += mouseMotion.Relative;
            }
        }

        //mouse motion click/drag detection threshold
        /*
        if (MotionAccumulation.Length() > 1.2)
        {
            dragging = true;
            Active = false;
            
        } */
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

        if(Debugging) GD.Print(data.Name);

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
        Tween tweenscale = GetTree().CreateTween();
        tweenscale.TweenProperty(this, "scale", this.originalScale, 0.2);
        switch (this.data.CardType)
        {   
            case CardType.Tower:
            case CardType.Chunk:
            case CardType.Spell:
                SpawnPlaceable();
                break;
            case CardType.Action:
                ActivateAction();
                break;
        }
    }

    private void ActivateAction()
    {
        ActionManager.GetInstance().AddAction(CardLoadingManager.GetInstance().GetPackedScene(this.data.SubjectScene).Instantiate<BaseAction>());
        EmitSignal("Placed", this);
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

    public void Discard()
    {
        DeckManager.GetInstance().Discard(data);
        QueueFree();
    }

}
