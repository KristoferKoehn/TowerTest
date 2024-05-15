using Godot;
using Godot.Collections;
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
                    node.Position = new Vector3(i - 3, 0, j - 3);
                    node.Name = $"{i - 3}, {j - 3}";

                }
            }
        }


        if (EditorInterface.Singleton.GetSelection().GetSelectedNodes().Count == 1 && !EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name.ToString().StartsWith("Chunk"))
        {
            if (Input.IsKeyPressed(Key.Up))
            {
                string nName = EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name;
                
                Vector2 pos = new Vector2(int.Parse(nName.Split(',')[0]), int.Parse(nName.Split(",")[1]));

                if (pos.Y >= -2 && pos.Y <= 3)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(GetTree().EditedSceneRoot.GetNode($"Chunk/{pos.X}, {pos.Y - 1}"));
                }
            }

            if (Input.IsKeyPressed(Key.Left))
            {
                string nName = EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name;

                Vector2 pos = new Vector2(int.Parse(nName.Split(',')[0]), int.Parse(nName.Split(",")[1]));

                if (pos.X >= -2 && pos.X <= 3)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(GetTree().EditedSceneRoot.GetNode($"Chunk/{pos.X - 1}, {pos.Y}"));
                }
            }

            if (Input.IsKeyPressed(Key.Right))
            {
                string nName = EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name;

                Vector2 pos = new Vector2(int.Parse(nName.Split(',')[0]), int.Parse(nName.Split(",")[1]));

                if (pos.X >= -3 && pos.X <= 2)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(GetTree().EditedSceneRoot.GetNode($"Chunk/{pos.X + 1}, {pos.Y}"));
                }
            }

            if (Input.IsKeyPressed(Key.Down))
            {
                string nName = EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name;

                Vector2 pos = new Vector2(int.Parse(nName.Split(',')[0]), int.Parse(nName.Split(",")[1]));

                if (pos.Y >= -3 && pos.Y <= 2)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(GetTree().EditedSceneRoot.GetNode($"Chunk/{pos.X}, {pos.Y + 1}"));
                }
            }

            if (Input.IsKeyPressed(Key.Key1))
            {
                MeshInstance3D MeshInstance = (MeshInstance3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];


                //make shorter
                ((BoxMesh)MeshInstance.Mesh).Size = new Vector3(1, 0.9f, 1);
                MeshInstance.Position = new Vector3(MeshInstance.Position.X, (0.9f / 2f) -0.5f, MeshInstance.Position.Z);
                MeshInstance.SetMeta("height", 0);

                MeshInstance.Mesh.SurfaceSetMaterial(0, GD.Load<Material>("res://Assets/Materials/LaneMaterial.tres"));

            }

            if (Input.IsKeyPressed(Key.Key2))
            {
                MeshInstance3D MeshInstance = (MeshInstance3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];


                //make shorter
                ((BoxMesh)MeshInstance.Mesh).Size = new Vector3(1, 1, 1);
                MeshInstance.Position = new Vector3(MeshInstance.Position.X, 1f / 2f - 0.5f, MeshInstance.Position.Z);
                MeshInstance.SetMeta("height", 1);

                MeshInstance.Mesh.SurfaceSetMaterial(0, GD.Load<Material>("res://Assets/Materials/GrassMaterial.tres"));
            }

            if (Input.IsKeyPressed(Key.Key3))
            {
                MeshInstance3D MeshInstance = (MeshInstance3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];


                //make shorter
                ((BoxMesh)MeshInstance.Mesh).Size = new Vector3(1, 1.1f, 1);
                MeshInstance.Position = new Vector3(MeshInstance.Position.X, 1.1f / 2f - 0.5f, MeshInstance.Position.Z);
                MeshInstance.SetMeta("height", 2);

                MeshInstance.Mesh.SurfaceSetMaterial(0, GD.Load<Material>("res://Assets/Materials/GrassMaterial.tres"));
            }

            if (Input.IsKeyPressed(Key.Key4))
            {
                MeshInstance3D MeshInstance = (MeshInstance3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];


                //make shorter
                ((BoxMesh)MeshInstance.Mesh).Size = new Vector3(1, 1.2f, 1);
                MeshInstance.Position = new Vector3(MeshInstance.Position.X, 1.2f / 2f - 0.5f, MeshInstance.Position.Z);
                MeshInstance.SetMeta("height", 3);

                MeshInstance.Mesh.SurfaceSetMaterial(0, GD.Load<Material>("res://Assets/Materials/GrassMaterial.tres"));
            }

        }

    }

}
