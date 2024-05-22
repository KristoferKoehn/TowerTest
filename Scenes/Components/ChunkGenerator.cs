using Godot;
using Godot.Collections;
using System;

[Tool]
public partial class ChunkGenerator : Node3D
{
    PackedScene TileScene = GD.Load<PackedScene>("res://Scenes/Components/Tile.tscn");

    const int DEFAULT_CHUNK_SIZE = 7;

    private Array<MeshInstance3D> LineMeshes = new Array<MeshInstance3D>();


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
            chunk.Owner = GetTree().EditedSceneRoot;

            //((Chunk)chunk).ChunkSize = DEFAULT_CHUNK_SIZE;

            for (int i = 0; i < DEFAULT_CHUNK_SIZE; i++) {
                
                for (int j = 0; j < DEFAULT_CHUNK_SIZE; j++)
                {
                    
                    Node3D node = TileScene.Instantiate<Node3D>();
                    GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(node);
                    node.Owner = GetTree().EditedSceneRoot;
                    node.Position = new Vector3(i - DEFAULT_CHUNK_SIZE/2, 0, j - DEFAULT_CHUNK_SIZE/2);
                    node.Name = $"{i - DEFAULT_CHUNK_SIZE/2}, {j - DEFAULT_CHUNK_SIZE / 2}";

                }
            }



            Spawner North = GD.Load<PackedScene>("res://Scenes/Components/Spawner.tscn").Instantiate<Spawner>();
            North.Name = "SpawnerNorth";
            GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(North);
            North.Owner = GetTree().EditedSceneRoot;
            North.Position = new Vector3(0, 0, -4);
            North.RotateY(Mathf.Pi);

            Spawner South = GD.Load<PackedScene>("res://Scenes/Components/Spawner.tscn").Instantiate<Spawner>();
            South.Name = "SpawnerSouth";
            GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(South);
            South.Owner = GetTree().EditedSceneRoot;
            South.Position = new Vector3(0, 0, 4);

            Spawner East = GD.Load<PackedScene>("res://Scenes/Components/Spawner.tscn").Instantiate<Spawner>();
            East.Name = "SpawnerEast";
            GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(East);
            East.Owner = GetTree().EditedSceneRoot;
            East.Position = new Vector3(4, 0, 0);
            East.RotateY(Mathf.Pi/2);

            Spawner West = GD.Load<PackedScene>("res://Scenes/Components/Spawner.tscn").Instantiate<Spawner>();
            West.Name = "SpawnerWest";
            GetTree().EditedSceneRoot.GetNode("Chunk").AddChild(West);
            West.Owner = GetTree().EditedSceneRoot;
            West.Position = new Vector3(-4, 0, 0);
            West.RotateY(-Mathf.Pi / 2);



            chunk.SetScript(GD.Load<Script>("res://Scenes/Components/Chunk.cs"));
        }


        if (EditorInterface.Singleton.GetSelection().GetSelectedNodes().Count == 1 && !EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0].Name.ToString().StartsWith("Chunk"))
        {
            if (Input.IsKeyPressed(Key.Up))
            {
                Node3D n = (Node3D)EditorInterface.Singleton.GetSelection().GetSelectedNodes()[0];
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(0, 0, -1), collisionMask: 8);
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
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(-1, 0, 0), collisionMask: 8);
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
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(1, 0, 0), collisionMask: 8);
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
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(n.GlobalPosition, n.GlobalPosition + new Vector3(0, 0, 1), collisionMask: 8);
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
