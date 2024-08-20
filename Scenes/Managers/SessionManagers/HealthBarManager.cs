using Godot;
using Managers;
using System;

public partial class HealthBarManager : Node
{
	private static HealthBarManager instance;
	private PackedScene HealthBarScene = GD.Load<PackedScene>("res://Scenes/UI/HealthBar.tscn");

	private HealthBarManager() { }

    public static HealthBarManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new HealthBarManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "HealthBarManager";
        }
        return instance;
    }
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		EnemyManager.GetInstance().Spawned += AttachHealthBar;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void AttachHealthBar(BaseEnemy enemy)
	{
		HealthBar temp = HealthBarScene.Instantiate<HealthBar>();
        enemy.AddChild(temp);
        enemy.DamageTaken += temp.UpdateHealthBar;
	}
}
