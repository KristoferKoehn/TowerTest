using Godot;


namespace MMOTest.scripts.Managers
{
    public partial class WaveManager : Node
    {
        private static WaveManager instance = null;

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
    }
}
