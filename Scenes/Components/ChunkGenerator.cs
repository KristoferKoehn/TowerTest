using Godot;
using System;

[Tool]
public partial class ChunkGenerator : Node3D
{
    PackedScene TileScene = GD.Load<PackedScene>("res://Scenes/Components/Tile.tscn");

    [Export]
    bool generate = false;


    public override void _Process(double delta)
    {
        if (generate)
        {
            generate = false;

            Node3D chunk = new Node3D();
            GetTree().EditedSceneRoot.AddChild(chunk);
            chunk.Name = "Chunk";
            chunk.SetScript(GD.Load<Script>("res://Scenes/Components/Chunk.cs"));
            chunk.Owner = GetTree().EditedSceneRoot;


            GD.Print("button pressed");

            for (int i = 0; i < 7; i++) {
                
                for (int j = 0; j < 7; j++)
                {
                    
                    Node3D node = TileScene.Instantiate<Node3D>();
                    GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(node);
                    node.Owner = GetTree().EditedSceneRoot;
                    node.Position = new Vector3(i-3, 0, j-3);

                }

            }

        }
    }

}
