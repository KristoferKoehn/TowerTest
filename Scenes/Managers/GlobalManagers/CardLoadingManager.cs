using Godot;
using System.Collections.Generic;

public partial class CardLoadingManager : Node
{
    private static CardLoadingManager instance;

    Dictionary<string, PackedScene> Towers = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Chunks = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Actions = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Weathers = new Dictionary<string, PackedScene>();
    Dictionary<Rarity, StyleBoxTexture> RarityStyles = new();
    Dictionary<string, CardData> CardData = new Dictionary<string, CardData>();

    private CardLoadingManager() { }

    public static CardLoadingManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new CardLoadingManager();
            if (SceneSwitcher.root != null)
            {
                SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
                instance.Name = "CardLoadingManager";
            }
        }
        return instance;
    }

    public override void _Ready()
    {
        //load in all subject scenes
        string[] TowerNames = DirAccess.GetFilesAt("res://Scenes/Towers/");
        foreach (string name in TowerNames)
        {
            if (name.Contains(".tscn"))
            {
                Towers.Add(name, GD.Load<PackedScene>($"res://Scenes/Towers/{name}"));
            }
        }

        string[] ChunkNames = DirAccess.GetFilesAt("res://Scenes/Chunks/");
        foreach (string name in ChunkNames)
        {
            if (name.Contains(".tscn"))
            {
                Chunks.Add(name, GD.Load<PackedScene>($"res://Scenes/Chunks/{name}"));
            }
        }

        string[] SpellNames = DirAccess.GetFilesAt("res://Scenes/Actions/");
        foreach (string name in SpellNames)
        {
            if (name.Contains(".tscn"))
            {
                Actions.Add(name, GD.Load<PackedScene>($"res://Scenes/Actions/{name}"));
            }
        }

        string[] WeatherNames = DirAccess.GetFilesAt("res://Scenes/Weather/");
        foreach (string name in WeatherNames)
        {
            if (name.Contains(".tscn"))
            {
                GD.Print(name);
                Weathers.Add(name, GD.Load<PackedScene>($"res://Scenes/Weather/{name}"));
            }
        }

        RarityStyles[Rarity.Common] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/CommonRarityStyleBox.tres");
        RarityStyles[Rarity.Uncommon] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/UncommonRarityStyleBox.tres");
        RarityStyles[Rarity.Rare] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/RareRarityStyleBox.tres");
        RarityStyles[Rarity.Epic] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/EpicRarityStyleBox.tres");
        RarityStyles[Rarity.Legendary] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/LegendaryRarityStyleBox.tres");

        string[] CardNames = DirAccess.GetFilesAt("res://Data/CardData/");
        foreach (string name in CardNames)
        {
            if (name.Contains(".tres"))
            {
                CardData.Add(name,ResourceLoader.Load<CardData>($"res://Data/CardData/{name}"));
            }
        }
    }

    public PackedScene GetPackedScene(string name)
    {
        PackedScene ps = null;
        if (Towers.ContainsKey(name))
        {

            ps = Towers[name];
        }
        else if (Chunks.ContainsKey(name))
        {
            ps = Chunks[name];
        }
        else if (Actions.ContainsKey(name))
        {
            ps = Actions[name];
        }
        else if (Weathers.ContainsKey(name))
        {
            ps = Weathers[name];
        }

        return ps;
    }

    public StyleBoxTexture GetRarityTexture(Rarity name)
    {
        return RarityStyles[name];
    }



    public List<CardData> GetAllCardData()
    {
        List<CardData> list = new();
        foreach(string name in CardData.Keys)
        {
            list.Add(CardData[name]);
        }
        
        return list;
    }
}
