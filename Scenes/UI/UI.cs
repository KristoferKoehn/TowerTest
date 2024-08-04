using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;

public partial class UI : CanvasLayer
{
    [Export]
    Label CurrencyLabel;
    [Export]
    private ProgressBar _mainTowerHealthBar;


    public static UI Instance { get; private set; }

    private float[] speedLevels = { 1.0f, 2.0f, 3.0f};
    private int currentSpeedIndex = 0;
    private ScrollContainer _towersPanel;
    private PackedScene _cardScene;
    private GridContainer _gridContainer; // The grid container holding the slots for the tower cards.
    private GameLoop _gameLoop; // Used for placing chunks (could probably be refactored)
 
    private StatBlock _playerStatBlock;
    private PlayerHand _playerHand;

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

        _playerHand = GetNode<PlayerHand>("Control/PlayerHand");
        _gameLoop = GetParent<GameLoop>();
        _towersPanel = GetNode<ScrollContainer>("Control/TowersPanel");
        PlayerStatsManager.GetInstance().StatChanged += GoldUpdate;
        GoldUpdate(StatType.Gold, PlayerStatsManager.GetInstance().GetStat(StatType.Gold));
    }


    private void SetUpChunkCardPanel()
    {
        // Cache the reference to the ChunksPanel node
        //_chunksPanel = GetNode<ScrollContainer>("Control/ChunksPanel");
       // _chunksPanel.Visible = false;
        _cardScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/Cards/BaseCard.tscn");
        _gridContainer = GetNode<GridContainer>("Control/ChunksPanel/GridContainer");

        //string[] ListOfChunks = DirAccess.GetFilesAt("res://Scenes/Chunks");

        foreach (string File in CardDatabase.chunkslist)
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

            // Get the Viewport from the slot
            Viewport viewport = slot.GetChild<Viewport>(0);

            // Get the card instance from the Viewport
            BaseCard card = viewport.GetChild<Control>(0) as BaseCard;

            if (card != null)
            {
                // Load and instantiate the tower
                Chunk newchunk = GD.Load<PackedScene>(card.ScenePath).Instantiate<Chunk>();
                newchunk.CurrentlyPlacing = true;
                newchunk.Debug = true;
                _gameLoop.AddChild(newchunk);

                // Close the panel
                _playerHand.Visible = !card.Visible;
            }
        }
    }
    

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        //_mainTowerHealthBar.Value = _playerStatBlock.GetStat(StatType.Health);
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

    public void _on_towers_button_pressed()
    {
        _playerHand.Visible = false;
        _towersPanel.Visible = !_towersPanel.Visible;
    }

    public void GoldUpdate(StatType type, float value)
    {
        
        if (type == StatType.Gold)
        {

            GD.Print($"Currency: {PlayerStatsManager.GetInstance().GetStat(type)}");
            CurrencyLabel.Text = $"Currency: {PlayerStatsManager.GetInstance().GetStat(type)}";
        }
    }

    public void _on_chunks_button_pressed()
    {
        //_towersPanel.Visible = false;
        _playerHand.Visible = !_playerHand.Visible;
    }
}
