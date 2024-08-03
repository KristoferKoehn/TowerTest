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


    public StatBlock statBlock = new();

    private PlayerStatsManager() {}

    public static PlayerStatsManager GetInstance()
    {
        if (instance == null)
        {
            instance = new PlayerStatsManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "PlayerStatsManager";
            Dictionary<StatType, float> sb = new();
            StatBlock.SetStatBlock(sb);
        }
        return instance;
    }

    public override void _Ready()
    {
        // Setting the starting stats:
        this.statBlock.SetStat(StatType.Health, 100); // Setting the main tower's health to 100.
    }

    /// <summary>
    /// behaves as a += operator on the given stat. Negative values subtract.
    /// </summary>
    /// <param name="st"></param>
    /// <param name="value"></param>
    public void ChangeStat(StatType st, float value)
    {
        StatBlock.SetStat(st, StatBlock.GetStat(st) + value);
        EmitSignal("StatChanged", (int)st, StatBlock.GetStat(st));
    }

    public void SetStat(StatType stat, float value)
    {
        StatBlock.SetStat(stat, value);
        EmitSignal("StatChanged", (int)stat, StatBlock.GetStat(stat));
    }

    public float GetStat(StatType stat)
    {
        return StatBlock.GetStat(stat);
    }

}
