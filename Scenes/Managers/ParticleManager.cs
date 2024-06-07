using Godot;
using System;
using System.Collections.Generic;

public partial class ParticleManager : Node
{

    private static ParticleManager instance;

    public List<GpuParticles3D> Particles = new();

    private ParticleManager() { }

    public static ParticleManager GetInstance()
    {
        if (instance == null)
        {
            instance = new ParticleManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "ParticleManager";
        }
        return instance;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void RegisterParticle(GpuParticles3D p)
    {
        Particles.Add(p);
    }

    public void UnregisterParticle(GpuParticles3D p)
    {
        Particles.Remove(p);
    }
}
