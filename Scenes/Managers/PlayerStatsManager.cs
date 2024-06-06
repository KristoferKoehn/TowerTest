using Godot;
using MMOTest.Backend;
using System;
using System.Collections.Generic;

public partial class PlayerStatsManager : Node
{
    private static PlayerStatsManager instance;

    StatBlock statBlock = new();

    private PlayerStatsManager() { }

    public static PlayerStatsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new PlayerStatsManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "PlayerStatsManager";
        }
        return instance;
    }

    public override void _Ready()
    {
        Dictionary<StatType, float> sb = new()
        {

        };
    }

}
