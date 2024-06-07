using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;

public partial class BaseEnemy : PathFollow3D
{

    [Signal]
    public delegate void DamageTakenEventHandler(Node self, Node source);

    [Signal]
    public delegate void DiedEventHandler(Node self);

    public StatBlock StatBlock = new();
	protected string ModelName;

	int ChunkCounter = 0;

	//stats, health whatever
	//speed

	//a spawner somewhere (not here)


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Loop = false;
		EnemyManager.GetInstance().RegisterEnemy(this);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if(StatBlock.GetStat(StatType.Health) <= 0.00f)
		{
			EmitSignal("Died",this);
			GD.Print("Dead");
			//change this later
			EnemyManager.GetInstance().UnregisterEnemy(this);
			QueueFree();

		}

		if (ProgressRatio == 1)
		{
			AttachNextPath();


			//switch paths
			//raycasting down (to hit the tile), asking for it's parent, then passing the tile into the chunk's thingy (GetPathsFromEntrance)
			//reparent to new path, set progress ratio to 0
		}

		Progress += 1.6f * (float)delta;
	}

	public MeshInstance3D GetTileAt(Vector3 to, Vector3 from)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(to, from, collisionMask: 8);
		Dictionary result = spaceState.IntersectRay(query);

		if (result.Count > 1)
		{
			return ((StaticBody3D)result["collider"]).GetParent<MeshInstance3D>();
		}
		else
		{
			return null;
		}
	}

	public Chunk GetChunkReferenceFromTile(MeshInstance3D tile)
	{
		return tile.GetParent<Chunk>();
	}

	public void AttachNextPath()
	{
		ChunkCounter += 1;

        MeshInstance3D temp = GetTileAt(ToGlobal(new Vector3(0,1,0)), ToGlobal(new Vector3(0, -1, 0)));
		Chunk chunk = GetChunkReferenceFromTile(temp);
		Array<Path3D> paths = chunk.GetPathsFromEntrance(temp);

		if (paths != null)
		{
			Random random = new Random();
			int randomIndex = random.Next(paths.Count);
			Reparent(paths[randomIndex]);
			ProgressRatio = 0;
		} else
		{
			EnemyManager.GetInstance().UnregisterEnemy(this);
			QueueFree();
		}

	}

	// Rough of what we might do
	public void TakeDamage(int damage)
	{
		if (damage <= 0) { return; }

		float CurrentHealth = this.StatBlock.GetStat(StatType.Health);
		float NewHealth = CurrentHealth - damage;
		if (NewHealth > 0)
		{
			this.StatBlock.SetStat(StatType.Health, CurrentHealth - damage);
		}
		else
		{
			// Die
		}
	}

	public float GetProgress()
	{
		return ChunkCounter + ProgressRatio;
	}

	public void PlayAnimation(string AnimationName)
	{
		GetNode<Node3D>(this.ModelName).GetNode<AnimationPlayer>("AnimationPlayer").Play(AnimationName);
	}

}
