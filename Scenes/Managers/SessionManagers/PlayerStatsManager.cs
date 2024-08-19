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

    public StatBlock StatBlock = new();

    private PlayerStatsManager() {}

    public static PlayerStatsManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new PlayerStatsManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "PlayerStatsManager";
            Dictionary<StatType, float> sb = new()
            {
                {StatType.Health, 100 },
                {StatType.HandSize, 7 },
                {StatType.Score, 0 },
            };

            instance.StatBlock.SetStatBlock(sb);

        }
        return instance;
    }

    public override void _Ready()
    {
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
