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

	public HealthBar healthBar;

	[Export] public AudioStreamPlayer3D StrikeSound { get; set; }

	public StatBlock StatBlock = new StatBlock();
	protected string ModelName;

	public int ChunkCounter = 0;
	public bool Disabled = false;
    public bool dead = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Loop = false;
        if (Disabled) return;
        EnemyManager.GetInstance().RegisterEnemy(this);

        HealthBar temp = GD.Load<PackedScene>("res://Scenes/UI/HealthBar.tscn").Instantiate<HealthBar>();
		this.healthBar = temp;
        this.AddChild(temp);
        this.DamageTaken += temp.UpdateHealthBar;
    }

    public void AddStatusEffect(BaseStatusEffect effect)
    {
        AddChild(effect);
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (Disabled) return;

        if (ProgressRatio == 1)
		{
			AttachNextPath();


			//switch paths
			//raycasting down (to hit the tile), asking for it's parent, then passing the tile into the chunk's thingy (GetPathsFromEntrance)
			//reparent to new path, set progress ratio to 0
		}
        Progress += StatBlock.GetStat(StatType.Speed) * (float)delta;
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

		MeshInstance3D temp = GetTileAt(ToGlobal(new Vector3(0,1,0)), ToGlobal(new Vector3(0, -1, 0)));
		Chunk chunk = GetChunkReferenceFromTile(temp);
        ChunkCounter = chunk.ChunkDistance;
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
			DamagePlayer();

            QueueFree();
		}

	}

	public void TakeDamage(float damage, Node source)
	{
		if (damage <= 0) { return; }
        float CurrentHealth = this.StatBlock.GetStat(StatType.Health);
		float NewHealth = CurrentHealth - damage;
        this.StatBlock.SetStat(StatType.Health, NewHealth);

		if (NewHealth <= 0)
		{
			Die();
        }
        EmitSignal("DamageTaken", this, source);
		Vector2 damage_num_position = this.GetViewport().GetCamera3D().UnprojectPosition(this.GlobalPosition);
		DamageNumbers.GetInstance().DisplayDamageNumbers(damage, damage_num_position, false);
	}

	public float GetProgress()
	{
		return ChunkCounter - ProgressRatio;
	}

	public void Die()
	{
		if (dead) return;

		dead = true;
		StatBlock.SetStat(StatType.Speed, 0);
        EnemyManager.GetInstance().UnregisterEnemy(this);
		
        EmitSignal("Died", this);

		// Give gold:
		AccountStatsManager.GetInstance().ChangeStat(StatType.Gold, this.StatBlock.GetStat(StatType.Gold));
		// Give score:
		PlayerStatsManager.GetInstance().ChangeStat(StatType.Score, 1); // just add 1 for now.


        GetNode<AnimationPlayer>("AnimationPlayer").SpeedScale = 2;
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Death_A");
		GetNode<AnimationPlayer>("AnimationPlayer").AnimationFinished += (StringName anim) =>
		{
			QueueFree();
        };
    }

	public void DamagePlayer()
	{
		PlayerStatsManager.GetInstance().ChangeStat(StatType.Health, -this.StatBlock.GetStat(StatType.Damage));
	}
}
