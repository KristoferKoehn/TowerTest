using Godot;
using System.Collections.Generic;

public partial class EnemyManager : Node
{

    private static EnemyManager instance;

    public List<BaseEnemy> Enemies = new();

    private EnemyManager() { }

    public static EnemyManager GetInstance()
    {
        if (instance == null)
        {
            instance = new EnemyManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "EnemyManager";
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

    public void RegisterEnemy(BaseEnemy enemy)
    {
        Enemies.Add(enemy);
    }

    public void UnregisterEnemy(BaseEnemy enemy)
    {
        Enemies.Remove(enemy);
    }

}
