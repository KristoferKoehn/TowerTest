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
	public Path3D CurrentPath { get; set; }

	public StatBlock StatBlock = new StatBlock();
	protected string ModelName;

	public int ChunkCounter = 0;
	public bool Disabled = false;
    public bool dead = false;
	public Array<int> ForkDecisions = new Array<int>();

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
		Random r = new Random();
		for(int i = 0; i < 64; i++)
		{

		}
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

        MeshInstance3D meshInstance3D = new MeshInstance3D();
        meshInstance3D.TopLevel = true;
        ImmediateMesh immediateMesh = new();
        OrmMaterial3D material = new();
        meshInstance3D.Mesh = immediateMesh;
        meshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
        immediateMesh.SurfaceAddVertex(to);
        immediateMesh.SurfaceAddVertex(from);

        immediateMesh.SurfaceEnd();
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;


        if (result.Count > 1)
        {
            material.AlbedoColor = Colors.Red;
            AddChild(meshInstance3D);
        }
        else
        {
            material.AlbedoColor = Colors.LimeGreen;
            AddChild(meshInstance3D);
        }

        Timer t = new Timer();
        meshInstance3D.AddChild(t);
        t.Start(0.3);
        t.Timeout += () =>
        {
            meshInstance3D.QueueFree();
            t.QueueFree();
        };

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

        Array<Path3D> paths = null;

        Chunk chunk = GetParent().GetParent() as Chunk;
		Spawner spawner = GetParent() as Spawner;
		if(chunk != null && spawner == null)
		{
			paths = chunk.NextPaths;
            ChunkCounter = chunk.ChunkDistance;
        } else if (spawner != null)
		{
			paths = spawner.NextPaths;
            ChunkCounter = int.MaxValue;
        }

		if (paths != null && paths.Count > 0)
		{
			Random random = new Random();
			int randomIndex = random.Next(paths.Count);
			CurrentPath = paths[randomIndex];
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

	public float GetTotalProgress()
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
		PlayerStatsManager.GetInstance().ChangeStat(StatType.Health, -StatBlock.GetStat(StatType.Damage));
	}

}
