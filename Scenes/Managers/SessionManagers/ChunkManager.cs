using Godot;
using MMOTest.Backend;
using System;

public partial class ChunkManager : Node
{
    private static ChunkManager instance;

    private ChunkManager() { }

    public StandardMaterial3D InvalidMaterial;
    public StandardMaterial3D InvalidLaneMaterial;
    public StandardMaterial3D ValidMaterial;
    public StandardMaterial3D ValidLaneMaterial;

    public static ChunkManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new ChunkManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "ChunkManager";
        }
        return instance;
    }

    public override void _Ready()
    {
        InvalidMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryInvalid.tres");
        InvalidLaneMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryInvalidLane.tres");
        ValidMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryValid.tres");
        ValidLaneMaterial = GD.Load<StandardMaterial3D>("res://Assets/Materials/QueryValidLane.tres");
    }

}
