using Godot;
using System;

public partial class ParticleWrapper : Node
{
	GpuParticles3D particle;
	//public bool readyToFire = true;

	public ParticleWrapper(GpuParticles3D p)
	{ 
		particle= p;
		particle.Emitting = false;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//particle.Finished += resetReady;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (particle.OneShot == true && particle.Emitting == false)
		{ 
			this.QueueFree();
		}
	}

	public void emitEffect(Vector3 position, Vector3 rotation)
	{ 
		//readyToFire= false;
		particle.Emitting= true;
	}

/*	private void resetReady()
	{ 
		readyToFire= true;
	}*/
}
