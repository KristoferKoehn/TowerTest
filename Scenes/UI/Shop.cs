using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class Shop : Control
{
    private GridContainer _chunkGridContainer;
    private GridContainer _towerGridContainer;
    private GridContainer _artifactGridContainer;
    private PackedScene _cardScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _cardScene = (PackedScene)ResourceLoader.Load("res://Scenes/UI/Cards/BaseCard.tscn");
        _chunkGridContainer = GetNode<GridContainer>("Panel/ScrollContainer/VBoxContainer/Chunks/VBoxContainer/HBoxContainer/ScrollContainer/GridContainer");
        _towerGridContainer = GetNode<GridContainer>("Panel/ScrollContainer/VBoxContainer/Towers/VBoxContainer/HBoxContainer/ScrollContainer/GridContainer");
        _artifactGridContainer = GetNode<GridContainer>("Panel/ScrollContainer/VBoxContainer/Artifacts/VBoxContainer/HBoxContainer/ScrollContainer/GridContainer");

        this.SetUpShop();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    private void SetUpShop()
    {
        this.SetupGridContainer(_chunkGridContainer, CardDatabase.chunkslist);
        this.SetupGridContainer(_towerGridContainer, CardDatabase.towerslist);
        //this.SetupGridContainer(_artifactGridContainer, CardDatabase.artifactlist);
    }

    private void SetupGridContainer(GridContainer _gridContainer, List<string> cards)
    {
        //string[] ListOfChunks = DirAccess.GetFilesAt("res://Scenes/Chunks");

        foreach (string File in cards)
        {
            VBoxContainer vBoxContainer = new VBoxContainer();
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
            Label costLabel = new Label();
            costLabel.Text = CardDatabase.DATA[File][2] + " G";
            costLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vBoxContainer.AddChild(slot);
            vBoxContainer.AddChild(costLabel);

            _gridContainer.AddChild(vBoxContainer);
        }
    }

    public void OnChunkCardClicked(InputEvent @event, TextureRect slot)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GD.Print("Clicked on a chunk card.");
        }
    }
}
