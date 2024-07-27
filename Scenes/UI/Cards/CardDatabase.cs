using Godot;
using System;
using System.Collections.Generic;

public static partial class CardDatabase
{
    public static readonly List<string> chunkslist = new List<string>
    {
        "res://Scenes/Chunks/c_2EAngle.tscn",
        "res://Scenes/Chunks/c_2EAngleB.tscn",
        "res://Scenes/Chunks/c_2EntranceForkA.tscn",
        "res://Scenes/Chunks/c_2EntranceSnakeA.tscn",
        "res://Scenes/Chunks/c_4EB.tscn",
        "res://Scenes/Chunks/c_ForkWithCycle.tscn",
        "res://Scenes/Chunks/c_StartingChunk.tscn",
        "res://Scenes/Chunks/c_TestChunk.tscn",
        "res://Scenes/Chunks/Laneless.tscn",
    };

    public static readonly List<string> towerslist = new List<string>
    {
        "res://Scenes/Towers/Ballista.tscn",
        //"LaserCrystal",
    };

    // Type, Resources,
    public static readonly Dictionary<string, List<string>> DATA = new Dictionary<string, List<string>>
    {
        { "res://Scenes/Chunks/c_2EAngle.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/c_2EAngleB.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/c_2EntranceForkA.tscn", new List<string> { "Chunks", "Uncommon" } },
        { "res://Scenes/Chunks/c_2EntranceSnakeA.tscn", new List<string> { "Chunks", "Uncommon" } },
        { "res://Scenes/Chunks/c_4EB.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/c_ForkWithCycle.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/c_StartingChunk.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/c_TestChunk.tscn", new List<string> { "Chunks", "Common" } },
        { "res://Scenes/Chunks/Laneless.tscn", new List<string> { "Chunks", "Rare" } },
        { "Ballista", new List<string> { "Towers", "Common"} },
        { "LaserCrystal", new List<string> {"Towers", "Epic"} },
    };
}
