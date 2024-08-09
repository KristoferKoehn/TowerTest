using Godot;
using System;
using System.Collections.Generic;

public partial class BallistaArrowManager : Node
{

	PackedScene ArrowScene = GD.Load<PackedScene>("res://Scenes/Components/BalistaArrow.tscn");

	static BallistaArrowManager instance;

	List<Ballista> ballistas = new();
	List<MeshInstance3D> Arrows = new();

	public static BallistaArrowManager GetInstance()
	{
		if (instance == null)
		{
			instance = new BallistaArrowManager();
			SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
			instance.Name = "BallistaArrowManager";
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
        foreach(MeshInstance3D arrow in Arrows)
        {
            arrow.TranslateObjectLocal(new Vector3(0, 0, -80.2f * (float)delta));
        }
	}

	public void RegisterBallista(Ballista ballista)
	{
        if (ballistas.Contains(ballista))
        {
            return;
        }
		ballistas.Add(ballista);
		ballista.TowerFired += (Node3D tower, Node3D target) => ShootArrow(tower, target);
	}

	public void UnregisterBallista(Ballista ballista)
	{
        if (ballistas.Contains(ballista))
        {
            ballistas.Remove(ballista);
        }
	}

    public void ShootArrow(Node3D tower, Node3D target)
    {
        //make arrow at tower (in the right spot)
        //
        MeshInstance3D arrow = ArrowScene.Instantiate<MeshInstance3D>();
        arrow.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
            //overdamage protection, prevents damage being dealt to multiple enemies erroneously 
            if (!arrow.HasMeta("struck"))
            {
                arrow.SetMeta("struck", "");
                arrow.GetNode<Area3D>("Hitbox").AreaEntered -= ((Ballista)tower).DealDamage;
                ParticleSignals.GetInstance().createParticle("CandyParticle", arrow.GlobalPosition, arrow.GlobalRotation);
            }
            Arrows.Remove(arrow);
            arrow.QueueFree();
        };
        SceneSwitcher.root.AddChild(arrow);
        Timer t = new Timer();
        arrow.AddChild(t);
        t.Start(4);
        t.OneShot = true;
        t.Timeout += () => { 
            if (arrow != null && !arrow.IsQueuedForDeletion())
            {
                Arrows.Remove(arrow);
                arrow.QueueFree();
            }
            t.QueueFree();
        };
        arrow.GetNode<Area3D>("Hitbox").AreaEntered += ((Ballista)tower).DealDamage;
        arrow.GlobalPosition = tower.GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow").GlobalPosition;
        arrow.LookAt(target.GlobalPosition + new Vector3(0, 0.5f, 0));
        Arrows.Add(arrow);
        arrow.SetMeta("target", target.GetPath());
    }
}
