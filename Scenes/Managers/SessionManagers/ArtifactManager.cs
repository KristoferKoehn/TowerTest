using Godot;
using System;
using System.Collections.Generic;

public partial class ArtifactManager : Node
{
    static ArtifactManager instance;

    public List<BaseArtifact> activeArtifacts = new List<BaseArtifact>();

    public static ArtifactManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new ArtifactManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "ArtifactManager";
        }
        return instance;
    }

    public void AddArtifact(BaseArtifact artifact)
    {
        this.activeArtifacts.Add(artifact);
        this.AddChild(artifact);
    }
}
