using Godot;
using System;

public partial class RNGManager : Node
{

    static RNGManager instance;

    public RandomNumberGenerator rng;
    ulong seed;
    int state;

    public static RNGManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new RNGManager();
            SceneSwitcher.root.GetNode<SceneSwitcher>("SceneSwitcher").AddChild(instance);
            instance.Name = "RNGManager";
            instance.rng = new RandomNumberGenerator();
            instance.seed = instance.rng.Seed;
        }
        return instance;
    }

    public void SetSeed(ulong seed)
    {
        rng.Seed = seed;
    }

    public ulong GetSeed()
    {
        return seed;
    }
}
