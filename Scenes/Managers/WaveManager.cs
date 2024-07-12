using Godot;
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


        Dictionary<Timer,List<PackedScene>> WaveBuckets = new();

        PackedScene necromancer;
        PackedScene SkeletonMinion;

        bool WaveActive = false;

        int WaveNumber = 0;

        //some way to determine wave content
        List<Spawner> spawners = new List<Spawner>();

        private WaveManager() { }

        public static WaveManager GetInstance()
        {
            if (instance == null)
            {
                instance = new WaveManager();
                SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
                instance.Name = "WaveManager";
            }
            return instance;
        }

        public override void _Ready()
        {
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
            WaveNumber++;
            EmitSignal("WaveStarted", WaveNumber);
            //get valid spawners
            List<Spawner> ValidSpawners = spawners
            .Where(item => item.Enabled)
            .ToList();

            int enemyCount = 4 + WaveNumber + ValidSpawners.Count;
            //GD.Print($"{ValidSpawners.Count} valid spawners, {enemyCount} enemies per spawner");
            foreach (Spawner item in ValidSpawners)
            {
                List<PackedScene> enemy = new();
                GD.Print($"at {item.GlobalPosition}");
                for (int i = 0;  i < enemyCount; i++)
                {
                    enemy.Add(SkeletonMinion);
                }

                Timer t = new Timer();
                t.Timeout += () => {
                    if (WaveBuckets[t].Count > 0)
                    {
                        BaseEnemy be = WaveBuckets[t][0].Instantiate<BaseEnemy>();
                        be.ChunkCounter = item.ChunkDistance;
                        WaveBuckets[t].Remove(WaveBuckets[t][0]);
                        t.GetParent<Spawner>().GetNode<Path3D>("Path3D").AddChild(be);
                        GD.Print("Spawn Stuff");
                    } else
                    {
                        t.QueueFree();
                    }
                };

                WaveBuckets[t] = enemy;

                item.AddChild(t);
                t.Start(0.7f);
            }
        }
    }
}
