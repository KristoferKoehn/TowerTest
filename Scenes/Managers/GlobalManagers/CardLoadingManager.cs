using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardLoadingManager : Node
{
    private static CardLoadingManager instance;

    Dictionary<string, PackedScene> Towers = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Chunks = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Artifacts = new Dictionary<string, PackedScene>();
    Dictionary<string, PackedScene> Spells = new Dictionary<string, PackedScene>();
    Dictionary<string, StyleBoxTexture> RarityStyles = new();
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

        string[] ArtifactNames = DirAccess.GetFilesAt("res://Scenes/Artifacts/");
        foreach (string name in ArtifactNames)
        {
            if (name.Contains(".tscn"))
            {
                Artifacts.Add(name, GD.Load<PackedScene>($"res://Scenes/Artifacts/{name}"));
            }
        }

        string[] SpellNames = DirAccess.GetFilesAt("res://Scenes/Spells/");
        foreach (string name in SpellNames)
        {
            if (name.Contains(".tscn"))
            {
                Spells.Add(name, GD.Load<PackedScene>($"res://Scenes/Spells/{name}"));
            }
        }

        RarityStyles["Common"] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/CommonRarityStyleBox.tres");
        RarityStyles["Uncommon"] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/UncommonRarityStyleBox.tres");
        RarityStyles["Rare"] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/RareRarityStyleBox.tres");
        RarityStyles["Epic"] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/EpicRarityStyleBox.tres");
        RarityStyles["Legendary"] = GD.Load<StyleBoxTexture>("res://Scenes/UI/Cards/Gradients/LegendaryRarityStyleBox.tres");

        string[] CardNames = DirAccess.GetFilesAt("res://Scenes/CardData/");
        foreach (string name in CardNames)
        {
            if (name.Contains(".tres"))
            {
                CardData.Add(name,ResourceLoader.Load<CardData>($"res://Scenes/CardData/{name}"));
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
        else if (Artifacts.ContainsKey(name))
        {
            ps = Artifacts[name];
        }
        else if (Spells.ContainsKey(name))
        {
            ps = Spells[name];
        }
        return ps;
    }

    public StyleBoxTexture GetRarityTexture(string name)
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
