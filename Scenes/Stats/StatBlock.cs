using Godot;
using System;
using System.Collections.Generic;

public enum StatType
{
	//Enemy Stats
	MaxHealth,
	MaxArmor,
	Health,
	Speed,
	Armor,
	FireResist,

	Gold,
	Damage,


	//Tower Stats
	AttackSpeed, // The interval in seconds before it can attack again, so lower = faster.
	Range,
	SellValue,
	FireDamage,
	IceDamage,
	WaterDamage,
	HandSize,

	//Game Stats
	Score,
}

namespace MMOTest.Backend
{

	public class StatProperty
	{
		public StatType StatType { get; set; }
		public float Value { get; set; }

		public StatProperty()
		{

		}

		public StatProperty(StatType statType, float statValue)
		{
			this.StatType = statType;
			this.Value = statValue;
		}
	}


	public class StatBlock
	{

		private Dictionary<StatType, float> statblock = new Dictionary<StatType, float>();
		public StatBlock() { }

		public void SetStat(StatType statType, float value)
		{
			statblock[statType] = value;
		}

		public float GetStat(StatType statType)
		{
			return statblock.ContainsKey(statType) ? statblock[statType] : 0f;
		}

		public void SetStatBlock(Dictionary<StatType, float> sb)
		{
			/*
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                if (sb.ContainsKey(stat))
				{
					this.statblock[stat] = sb[stat];
				}
            }
			*/

            if (sb.ContainsKey(StatType.Health) && !sb.ContainsKey(StatType.MaxHealth)) 
			{
				sb[StatType.MaxHealth] = sb[StatType.Health];
			}

			statblock = sb;
		}
	}
}
