using Godot;
using Managers;
using MMOTest.Backend;
using System;
using System.Collections.Generic;

public partial class PlayerStatsManager : Node
{
    private static PlayerStatsManager instance;

    public StatBlock statBlock = new();

    private PlayerStatsManager() { }

    public static PlayerStatsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new PlayerStatsManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "PlayerStatsManager";
        }
        return instance;
    }

    public override void _Ready()
    {
        // Setting the starting stats:
        this.statBlock.SetStat(StatType.Health, 100); // Setting the main tower's health to 100.
    }
}
