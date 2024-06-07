using Godot;
using MMOTest.Backend;
using System;

public partial class ChunkManager : Node
{
    private static ChunkManager instance;

    Chunk placingChunk;

    private ChunkManager() { }

    public static ChunkManager GetInstance()
    {
        if (instance == null)
        {
            instance = new ChunkManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "ChunkManager";
        }
        return instance;
    }


    // probably do this differently. My intent is to have some way to tentatively place a chunk, and turn it trans red or blue depending on the validity of the placement.
    // Need to check all the directions of the entrances of the *tentative chunk* to look for collisions, and there can be only one connection to another lane (for nominal case)
    // this may need to be a state managed via chunk, or some other thing. It's not super clear here, however there should be a "chunk placed" signal emitted from here regardless.

    /*
     * Get chunk entrances
     * raycast each direction of entrances (from entrance position to one past, as in an entrance to the north (0, -7) to (0, -8) to check for adjacent chunk)
     * if adjacent chunks have no lane entrances matching this chunk entrances, invalid, change to red, do not accept
     * if adjacent chunks have more than one lane entrance maching this chunk entrances, also invalid (should prevent this case before this happens so we don't have to check)
     * if only one detected adjacent chunk and is lane entrance, place chunk and set chunk exit in global cardinal of adjacent chunk. that should be it.
     * 
     * the problem is that the chunk needs to be constantly checking validity, it can't be a simple function.
     */

    public void ChunkPlacingQuery(PackedScene chunkType, Vector3 position)
    {
        Chunk placingChunk = chunkType.Instantiate<Chunk>();
        placingChunk.Position = position;

        GetParent<GameLoop>().AddChild(placingChunk);

    }



}
