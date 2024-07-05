using Godot;
using Godot.Collections;
using System;

public partial class UI : CanvasLayer
{
    public static UI Instance { get; private set; }

    private float[] speedLevels = { 1.0f, 2.0f, 3.0f};
    private int currentSpeedIndex = 0;
    private ScrollContainer _cardsPanel;
    private ScrollContainer _towersPanel;
    private PackedScene _cardScene;
    private GridContainer _gridContainer; // The grid container holding the slots for the chunk cards.
    private GameLoop _gameLoop; // Used for placing chunks (could probably be refactored)

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Instance stuff
        if (Instance != null && Instance != this)
        {
            QueueFree();
        }
        else
        {
            Instance = this;
        }

        _gameLoop = GetParent<GameLoop>();
        SetUpChunkCardPanel();
        _towersPanel = GetNode<ScrollContainer>("Control/TowersPanel");
    }

    private void SetUpChunkCardPanel()
    {
        // Cache the reference to the CardsPanel node
        _cardsPanel = GetNode<ScrollContainer>("Control/CardsPanel");
        _cardsPanel.Visible = false;
        _cardScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/Cards/BaseCard.tscn");
        _gridContainer = GetNode<GridContainer>("Control/CardsPanel/GridContainer");

        string[] ListOfChunks = DirAccess.GetFilesAt("res://Scenes/Chunks");

        foreach (string File in ListOfChunks)
        {
            //Texture rect slot:

            TextureRect slot = new TextureRect();
            slot.CustomMinimumSize = new Vector2(125, 175);
            slot.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;

            // Create a Viewport:
            Viewport viewport = new SubViewport();
            viewport.Set("size", new Vector2(250, 350));
            viewport.TransparentBg = true;
            slot.AddChild(viewport);

            // Instance the card scene and add it to the viewport
            BaseCard card = _cardScene.Instantiate<BaseCard>();
            card.SetCard(File);
            viewport.AddChild(card);

            // Create a TextureRect to display the viewport texture
            slot.Texture = viewport.GetTexture();

            slot.GuiInput += (InputEvent @event) => {
                if (@event.IsAction("select"))
                {
                    OnChunkCardClicked(@event, slot);
                }

            };

            // Add the TextureRect to the GridContainer
            _gridContainer.AddChild(slot);
        }


        // Create and add cards to the GridContainer
        for (int i = 0; i < 10; i++)
        {
            
        }
    }

    public void OnChunkCardClicked(InputEvent @event, TextureRect slot)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GD.Print("Clicked on a chunk card.");

            // Get the Viewport from the slot
            Viewport viewport = slot.GetChild<Viewport>(0);

            // Get the card instance from the Viewport
            BaseCard card = viewport.GetChild<Control>(0) as BaseCard;

            if (card != null)
            {
                // Load and instantiate the chunk
                Chunk newchunk = GD.Load<PackedScene>(card.ChunkPath).Instantiate<Chunk>();
                newchunk.CurrentlyPlacing = true;
                newchunk.Debug = true;
                _gameLoop.AddChild(newchunk);

                // Close the panel
                _cardsPanel.Visible = !card.Visible;
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void _on_pause_play_button_pressed()
    {
        if (GetTree().Paused)
        {
            GetTree().Paused = false;
            GD.Print("Resuming");
        }
        else
        {
            GetTree().Paused = true;
            GD.Print("Pausing");
        }
    }

    public void _on_speed_up_button_pressed()
    {
        if (GetTree().Paused)
        {
            return;
        }

        // Move to the next speed level
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        Engine.TimeScale = speedLevels[currentSpeedIndex];

        GD.Print($"Setting speed to {Engine.TimeScale}x");
    }

    public void _on_settings_button_pressed()
    {
        //this.FindParent("SceneSwitcher").PushScene(GD.Load<PackedScene>("res://Scenes/menus/SettingsMenu.tscn").Instantiate<Control>());
        //pause
        //add child ()
    }

    public void _on_chunks_button_pressed()
    {
        // Toggle the visibility of the CardsPanel
        _cardsPanel.Visible = !_cardsPanel.Visible;
        _towersPanel.Visible = false;
    }

    public void _on_towers_button_pressed()
    {
        _cardsPanel.Visible = false;
        _towersPanel.Visible = !_towersPanel.Visible;
    }

}
