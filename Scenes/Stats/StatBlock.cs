using Godot;
using System.Collections.Generic;

public enum StatType
{
	//Enemy Stats
	Health,
	Speed,
	Armor,
	FireResist,


	Damage,


	//Tower Stats
	AttackSpeed,
	Range,
	SellValue,
	FireDamage,
	IceDamage,
	WaterDamage,


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
			statblock = sb;
		}
	}
}
