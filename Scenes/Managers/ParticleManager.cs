using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Enumeration;

public partial class ParticleManager : Node
{

	private static ParticleManager instance;

	//After messing around, doing an enumerable feels odd. New plan is to preload all our particles into a hashtable, then we can use that to nab the correct particle.
	//From there, just instantiate it and youre good to go.
	public List<ParticleWrapper> Particles = new();
	private ParticleSignals particleSignals = ParticleSignals.GetInstance();
	//private List<GpuParticles3D> loadedParticles = new();
	private Hashtable loadedParticles = new Hashtable();

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
		//GD.Print("hi");
		particleSignals.normalParticle += (String n, Vector3 c, Vector3 d) => makeNormalParticle(n, c, d);
		//loading particles
		foreach (string item in DirAccess.GetFilesAt("res://Scenes/Particles/"))
		{
			//GD.Print(item.Substring(0, item.Length - 5));
			if (item.Contains("tscn"))
			{
				PackedScene inst = GD.Load<PackedScene>("res://Scenes/Particles/" + item);
				//GpuParticles3D particleScene = inst.Instantiate<GpuParticles3D>();
				loadedParticles[item.Substring(0, item.Length-5)] = inst;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//will have to have a few of these or a switcher to tell where to put it.
	public void RegisterParticle(ParticleWrapper p)
	{
		Particles.Add(p);
	}

	public void UnregisterParticle(ParticleWrapper p)
	{
		Particles.Remove(p);
	}

	/*    private void fireP1(ParticleWrapper p, Vector3 c, Vector3 d)
		{
			p.emitEffect(c, d);
		}*/

	/*	private void makeP1(Vector3 c, Vector3 d)
		{
			var inst = GD.Load<PackedScene>("res://Scenes/Particles/Particle1.tscn");
			GpuParticles3D particleScene = inst.Instantiate<GpuParticles3D>();
			AddChild(particleScene);
			ParticleWrapper tempWraper = new ParticleWrapper(particleScene);
			Particles.Add(tempWraper);
			//fireP1(tempWraper, c, d);
			tempWraper.emitEffect(c, d);
		}*/

	private void makeNormalParticle(string name, Vector3 c, Vector3 d)
	{ 
		PackedScene inst = (PackedScene)loadedParticles[name];
		if (inst != null)
		{
			GpuParticles3D particleScene = inst.Instantiate<GpuParticles3D>();
			AddChild(particleScene);
			ParticleWrapper tempWraper = new ParticleWrapper(particleScene);
			Particles.Add(tempWraper);
			tempWraper.emitEffect(c, d);
		}
	}
}
