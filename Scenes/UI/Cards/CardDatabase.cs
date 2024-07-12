using Godot;
using System;
using System.Collections.Generic;

public static partial class CardDatabase
{
    public static readonly List<string> chunkslist = new List<string>
    {
        "c_2EAngle",
        "c_2EAngleB",
        "c_2EntranceForkA",
        "c_2EntranceSnakeA",
        "c_4EB",
        "c_ForkWithCycle",
        "c_StartingChunk",
        "c_TestChunk",
        "Laneless",
    };

    public static readonly List<string> towerslist = new List<string>
    {
        "Ballista",
        //"LaserCrystal",
    };

    // Type, Resources,
    public static readonly Dictionary<string, List<string>> DATA = new Dictionary<string, List<string>>
    {
        { "c_2EAngle", new List<string> { "Chunks", "Common" } },
        { "c_2EAngleB", new List<string> { "Chunks", "Common" } },
        { "c_2EntranceForkA", new List<string> { "Chunks", "Uncommon" } },
        { "c_2EntranceSnakeA", new List<string> { "Chunks", "Uncommon" } },
        { "c_4EB", new List<string> { "Chunks", "Common" } },
        { "c_ForkWithCycle", new List<string> { "Chunks", "Common" } },
        { "c_StartingChunk", new List<string> { "Chunks", "Common" } },
        { "c_TestChunk", new List<string> { "Chunks", "Common" } },
        { "Laneless", new List<string> { "Chunks", "Rare" } },
        { "Ballista", new List<string> { "Towers", "Common"} },
        { "LaserCrystal", new List<string> {"Towers", "Epic"} },
    };
}
