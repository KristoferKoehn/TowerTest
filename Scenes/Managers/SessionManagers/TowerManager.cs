using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using static Godot.TextServer;
using System.Threading.Tasks;
using MMOTest.Backend;

public partial class TowerManager : Node
{
    static TowerManager instance;

    public System.Collections.Generic.Dictionary<TowerType, List<AbstractTower>> activeTowers = new();

	public static TowerManager GetInstance()
	{
		if (!IsInstanceValid(instance))
		{
			instance = new TowerManager();
			SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
			instance.Name = "TowerManager";
		}
		return instance;
	}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Set up the tower dictionary for use.
        foreach(TowerType type in Enum.GetValues(typeof(TowerType)))
        {
            activeTowers.Add(type, new List<AbstractTower>());
        }
	}

	public void RegisterTower(AbstractTower tower)
	{
        TowerType type = tower.TowerType;

        List<AbstractTower> towerTypeList = activeTowers[type];
        if (towerTypeList.Contains(tower))
        {
            return;
        }
        towerTypeList.Add(tower);
	}

	public void UnregisterTower(AbstractTower tower)
	{
        TowerType type = tower.TowerType;
        List<AbstractTower> towerTypeList = activeTowers[type];
        if (towerTypeList.Contains(tower))
        {
            towerTypeList.Remove(tower);
        }
	}
}
