using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Managers
{
	public partial class WaveManager : Node
	{

		[Signal]
		public delegate void WaveStartedEventHandler(int WaveNumber);

		[Signal]
		public delegate void WaveEndedEventHandler();

		private static WaveManager instance;


		System.Collections.Generic.Dictionary<Timer,List<PackedScene>> WaveBuckets = new();
		System.Collections.Generic.Dictionary<string, PackedScene> EnemyScenes = new();

		PackedScene necromancer;
		PackedScene SkeletonMinion;

		bool WaveActive = false;

		int WaveNumber = 0;
		int ForkValue = 1;

		//some way to determine wave content
		List<Spawner> spawners = new List<Spawner>();

		Array<Array<Array<string>>> Level;

		private WaveManager() { }

        public static WaveManager GetInstance()
        {
            if (!IsInstanceValid(instance))
            {
                instance = new WaveManager();
                SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
                instance.Name = "WaveManager";
            }
            return instance;
        }

        public override void _Ready()
        {
            string[] names = DirAccess.GetFilesAt("res://Scenes/Enemies/");
            foreach (string name in names)
            {
                if(name.Contains(".tscn"))
                {
                    EnemyScenes[name] = GD.Load<PackedScene>($"res://Scenes/Enemies/{name}");
                }
            }

			//read in structure
			necromancer = GD.Load<PackedScene>("res://Scenes/Enemies/Necromancer.tscn");
			SkeletonMinion = GD.Load<PackedScene>("res://Scenes/Enemies/Skeleton.tscn");
		}

		public override void _Process(double delta)
		{
			if (WaveActive)
			{
				if(EnemyManager.GetInstance().Enemies.Count == 0)
				{

					bool incomingCheck = false;
					foreach (Timer t in WaveBuckets.Keys)
					{
						if(WaveBuckets[t].Count > 0)
						{
							incomingCheck = true;
						}
					}

					if (!incomingCheck)
					{
						WaveActive = false;
						//wave over
						EmitSignal("WaveEnded");
					}
				}
			}
		}

		public void RegisterSpawner(Spawner spawner)
		{
			spawners.Add(spawner);
		}

		public void DeregisterSpawner(Spawner spawner)
		{
			spawners.Remove(spawner);
		}

		public void StartWave()
		{
			WaveActive = true;
			Level = WaveDataManager.GetInstance().LoadWave("test.json");
			EmitSignal("WaveStarted", WaveNumber);
			//get valid spawners
			List<Spawner> ValidSpawners = spawners
			.Where(item => item.Enabled)
			.ToList();

			
			int enemyCount = 4 + WaveNumber + ValidSpawners.Count;

			List<List<PackedScene>> StagingBuckets = new();

			foreach(Spawner s in ValidSpawners)
			{
				StagingBuckets.Add(new List<PackedScene>());    
			}

			List<string> MainDistribution = new();
			MainDistribution.AddRange(Level[WaveNumber][0]);

			if (ForkValue > 0)
			{
				for (int i = 0; i <  ForkValue; i++)
				{
					MainDistribution.AddRange(Level[WaveNumber][0]);
				}
			}

			int distribution = 0;
			foreach (string s in MainDistribution)
			{
				StagingBuckets[distribution].Add(EnemyScenes[s]);

				distribution++;
				distribution = distribution % ValidSpawners.Count;
			}

			WaveNumber++;

			//distribute enemies from wave to buckets
			// if the fork value is larger than spawner count, distribute that amount of fork waves to buckets

			// add entire fork wave to each bucket

			int SpawnerIndex = 0;
			//GD.Print($"{ValidSpawners.Count} valid spawners, {enemyCount} enemies per spawner");
			foreach (Spawner item in ValidSpawners)
			{

				Timer t = new Timer();
				t.Timeout += () => {
					if (WaveBuckets[t].Count > 0)
					{
						BaseEnemy be = WaveBuckets[t][0].Instantiate<BaseEnemy>();
						be.ChunkCounter = item.ChunkDistance;
						WaveBuckets[t].Remove(WaveBuckets[t][0]);
						t.GetParent<Spawner>().AddChild(be);
						be.CurrentPath = t.GetParent<Spawner>();
						GD.Print("Spawn Stuff");
					} else
					{
						t.QueueFree();
					}
				};

				WaveBuckets[t] = StagingBuckets[SpawnerIndex];

				item.AddChild(t);
				t.Start(0.7f);
				SpawnerIndex++;
			}
		}
	}
}
