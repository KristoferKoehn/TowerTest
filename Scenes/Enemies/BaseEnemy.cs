using Godot;
using Godot.Collections;
using MMOTest.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public partial class BaseEnemy : PathFollow3D
{
	private bool Debugging = false;
    [Signal] public delegate void DamageTakenEventHandler(Node self, Node source);
	[Signal] public delegate void DiedEventHandler(Node self);
    [Export] public AudioStreamPlayer3D StrikeSound { get; set; }
	public HealthBar healthBar;
	public StatBlock StatBlock = new StatBlock();
	protected string ModelName;
	public int ChunkCounter = 0;
	public bool Disabled = false;
    public bool dead = false;
	public float TimeScale = 1.0f;

	public List<BaseStatusEffect> ActiveStatusEffects = new List<BaseStatusEffect>();

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
        // Check if an effect of the same type is already active
        if (this.ActiveStatusEffects.Any(e => e.GetType() == effect.GetType()))
        {
			if (Debugging) GD.Print("cannot apply same effect twice");
            return;
        }

        this.ActiveStatusEffects.Add(effect);
        AddChild(effect);
    }

	public void RemoveStatusEffect(BaseStatusEffect effect)
	{
        effect.IsActive = false;
        effect.effectIcon.QueueFree(); // Remove the effect icon which was reparented to the health bar
        this.ActiveStatusEffects.Remove(effect);
		RemoveChild(effect);
		effect.QueueFree();
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		delta *= this.TimeScale;
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

	public void TakeDamage(float damage, Node source, bool IsCrit, DamageType damageType)
	{
        // Example of damageTypeMultiplier or vulnerability per damage type
        float damageTypeMultiplier = GetDamageTypeMultiplier(damageType);

        // Adjust damage based on damageTypeMultiplier/vulnerability
        float adjustedDamage = damage * damageTypeMultiplier;

        if (adjustedDamage <= 0) { return; }
		float CurrentHealth = this.StatBlock.GetStat(StatType.Health);
		float NewHealth = CurrentHealth - adjustedDamage;
		this.StatBlock.SetStat(StatType.Health, NewHealth);

		if (NewHealth <= 0)
		{
			Die();
        }
        EmitSignal("DamageTaken", this, source);
		Vector2 damage_num_position = this.GetViewport().GetCamera3D().UnprojectPosition(this.GlobalPosition);
		DamageNumbers.GetInstance().DisplayDamageNumbers(damage, damage_num_position, IsCrit);
	}

    public float GetDamageTypeMultiplier(DamageType type)
    {
        switch (type)
        {
            case DamageType.Physical:
                return this.StatBlock.GetStat(StatType.PhysicalMultiplier);
            case DamageType.Fire:
                return this.StatBlock.GetStat(StatType.FireMultiplier);
			case DamageType.Water:
				return this.StatBlock.GetStat(StatType.WaterMultiplier);
			case DamageType.Shock:
				return this.StatBlock.GetStat(StatType.ShockMultiplier);
			case DamageType.Ice:
				return this.StatBlock.GetStat(StatType.IceMultiplier);
			case DamageType.Wind:
				return this.StatBlock.GetStat(StatType.WindMultiplier);
			case DamageType.Poison:
				return this.StatBlock.GetStat(StatType.PoisonMultiplier);
            default:
                return 1.0f; // Keep the same by default
        }
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
