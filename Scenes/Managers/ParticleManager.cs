using Godot;
using System;
using System.Collections.Generic;

public partial class ParticleManager : Node
{

    private static ParticleManager instance;

    //I think we are going to have to have multiple of the same particle effect unfortunately. While I can turn emitting on and off, it sounds like that will
    //force the currently rendered particles to die and thats a MASSIVE bummer. so, we'll have multiple different particle lists. Will do when we have
    //the emitters
    public List<ParticleWrapper> Particles = new();
    private ParticleSignals particleSignals = ParticleSignals.GetInstance();

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

    //will have to have a few of these or a switcher to tell where to put it.
    public void RegisterParticle(GpuParticles3D p)
    {
        Particles.Add(new ParticleWrapper(p));
    }

    public void UnregisterParticle(ParticleWrapper p)
    {
        Particles.Remove(p);
    }

    private void fireP1(Vector3 c, Vector3 d)
    {
        //currently written for the single list, so here's how it will work
        //go through the list until you hit a P1 that can emit.
        foreach (ParticleWrapper p in Particles)
        {
            if (p.readyToFire)
            {
                p.emitEffect(c, d);
                break;
            }
        }
    }
}
