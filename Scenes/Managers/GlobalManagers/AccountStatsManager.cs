using Godot;
using System.Collections.Generic;
using MMOTest.Backend;
using System;

public partial class AccountStatsManager : Node
{
    [Signal]
    public delegate void StatChangedEventHandler(StatType st, float value);

    private static AccountStatsManager instance;

    public StatBlock StatBlock = new();

    private AccountStatsManager() { }

    public static AccountStatsManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new AccountStatsManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "AccountStatsManager";
            Dictionary<StatType, float> sb = new()
            {
                {StatType.Gold, 200 },
            };

            instance.StatBlock.SetStatBlock(sb);

        }
        return instance;
    }

    public override void _Ready()
    {
        // Setting the starting stats:
        this.StatBlock.SetStat(StatType.Health, 100); // Setting the main tower's health to 100.
    }

    /// <summary>
    /// behaves as a += operator on the given stat. Negative values subtract.
    /// </summary>
    /// <param name="stat"></param>
    /// <param name="value"></param>
    public void ChangeStat(StatType stat, float value)
    {
        StatBlock.SetStat(stat, StatBlock.GetStat(stat) + value);
        EmitSignal("StatChanged", (int)stat, StatBlock.GetStat(stat));
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
