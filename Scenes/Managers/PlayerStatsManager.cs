using Godot;
using Managers;
using MMOTest.Backend;
using System;
using System.Collections.Generic;

public partial class PlayerStatsManager : Node
{


    [Signal]
    public delegate void StatChangedEventHandler(StatType st, float value);


    private static PlayerStatsManager instance;

    static StatBlock StatBlock = new();

    private PlayerStatsManager() { }

    public static PlayerStatsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new PlayerStatsManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "PlayerStatsManager";
        }
        Dictionary<StatType, float> sb = new()
        {
            { StatType.Gold, 0},
        };

        StatBlock.SetStatBlock(sb);


        return instance;
    }

    public override void _Ready()
    {
        
    }

    public void ChangeStat(StatType st, float value)
    {

    }

    public void SetStat(StatType stat, float value)
    {
        StatBlock.SetStat(stat, value);
        EmitSignal("StatChanged", (int)stat, value);
    }

}
