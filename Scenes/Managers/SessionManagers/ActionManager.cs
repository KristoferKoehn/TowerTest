using Godot;
using System.Collections.Generic;


public partial class ActionManager : Node
{
    private bool Debugging = true;

    static ActionManager instance;


    public static ActionManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new ActionManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "ActionManager";
        }
        return instance;
    }



    public void AddAction(InstantAction action)
    {
        //unimplemented for now

        //register action
        //have logic about the action
        action.ApplyEffect();
    }
}
