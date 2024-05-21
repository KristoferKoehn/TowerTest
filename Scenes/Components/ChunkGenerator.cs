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
                Node3D n = (Node3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(0, 0, -1));
                Dictionary result = spaceState.IntersectRay(query);

                if (result.Count > 1)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(((Node3D)result["collider"]).GetParent());
                }
            }

            if (Input.IsKeyPressed(Key.Left))
            {
                Node3D n = (Node3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(-1, 0, 0));
                Dictionary result = spaceState.IntersectRay(query);

                if (result.Count > 1)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(((Node3D)result["collider"]).GetParent());
                }
            }

            if (Input.IsKeyPressed(Key.Right))
            {
                Node3D n = (Node3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(1, 0, 0));
                Dictionary result = spaceState.IntersectRay(query);

                if (result.Count > 1)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(((Node3D)result["collider"]).GetParent());
                }
            }

            if (Input.IsKeyPressed(Key.Down))
            {
                Node3D n = (Node3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(0, 0, 1));
                Dictionary result = spaceState.IntersectRay(query);

                if (result.Count > 1)
                {
                    EditorInterface.Singleton.GetSelection().Clear();
                    EditorInterface.Singleton.GetSelection().AddNode(((Node3D)result["collider"]).GetParent());
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
