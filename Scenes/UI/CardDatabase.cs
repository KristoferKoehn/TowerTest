using Godot;
using System;
using System.Collections.Generic;

public static partial class CardDatabase
{
    public enum ChunkType
    {
        c_2EAngle,
        c_2EAngleB,
        c_EntranceForkA,
        c_EntranceSnakeA,
        c_EB,
        c_ForkWithCycle,
        c_StartingChunk,
        c_TestChunk
    }

    // Type, Resources,
    public static readonly Dictionary<ChunkType, List<string>> DATA = new Dictionary<ChunkType, List<string>>
    {
        { ChunkType.c_2EAngle, new List<string> { "Chunks" } },
        { ChunkType.c_2EAngleB, new List<string> { "Chunks" } },
        { ChunkType.c_EntranceForkA, new List<string> { "Chunks" } },
        { ChunkType.c_EntranceSnakeA, new List<string> { "Chunks" } },
        { ChunkType.c_EB, new List<string> { "Chunks" } },
        { ChunkType.c_ForkWithCycle, new List<string> { "Chunks" } },
        { ChunkType.c_StartingChunk, new List<string> { "Chunks" } },
        { ChunkType.c_TestChunk, new List<string> { "Chunks" } }
    };

    public static ChunkType Get(string cardName)
    {
        if (Enum.TryParse(cardName, true, out ChunkType chunkType))
        {
            return chunkType;
        }
        else
        {
            throw new ArgumentException($"Invalid card name: {cardName}");
        }
    }
}
