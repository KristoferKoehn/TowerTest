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
			if(arrow.HasMeta("target"))
			{
				if(((Node3D)arrow.GetMeta("target")).IsQueuedForDeletion())
				{
					arrow.RemoveMeta("target");
				} else
				{
					arrow.LookAt(((Node3D)arrow.GetMeta("target")).GlobalPosition);
				}
				
			}

			arrow.TranslateObjectLocal(new Vector3(0, 0, -0.9f));
		}
	}

	public void RegisterBallista(Ballista ballista)
	{
		ballistas.Add(ballista);
		ballista.TowerFired += (Node3D tower, Node3D target) => ShootArrow(tower, target);
	}

	public void UnregisterBallista(Ballista ballista)
	{
		ballistas.Remove(ballista);
	}

	public void ShootArrow(Node3D tower, Node3D target)
	{
		//make arrow at tower (in the right spot)
		//
		MeshInstance3D arrow = ArrowScene.Instantiate<MeshInstance3D>();
		arrow.GetNode<Area3D>("Hitbox").AreaEntered += (Area3D area) => {
			BaseEnemy be = area.GetParent<BaseEnemy>();
			be.StatBlock.SetStat(StatType.Health, be.StatBlock.GetStat(StatType.Health) - ((Ballista)tower).StatBlock.GetStat(StatType.Damage));

			//put logic here
			Arrows.Remove(arrow);
			arrow.QueueFree();

			ParticleSignals.GetInstance().dop1(arrow.GlobalPosition, arrow.GlobalRotation);
			//p.EmitSignal("p1", arrow.GlobalPosition, arrow.GlobalRotation);
		};
		SceneSwitcher.root.AddChild(arrow);
		arrow.GlobalPosition = tower.GetNode<MeshInstance3D>("weapon_ballista2/tmpParent/weapon_ballista/bow/arrow").GlobalPosition;
		arrow.LookAt(target.GlobalPosition);
		Arrows.Add(arrow);
		arrow.SetMeta("target", target);
	}
}
