using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TowerTest.Scenes.Components;

public partial class ActionManager : Node
{
    private bool Debugging = true;

    static ActionManager instance;

    public List<BaseAction> activeActions = new List<BaseAction>();

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

    public void AddAction(BaseAction action)
    {
        // Check if an action of the same type already exists
        if (this.activeActions.Any(a => a.GetType() == action.GetType()))
        {
            // Don't add the action if one of the same type already exists
            if(Debugging) GD.Print("already have that kind of action active.");
            return;
        }

        this.activeActions.Add(action);
        AddChild(action);
        action.ApplyEffect();
    }

    public void AddAction(InstantAction action)
    {
        //unimplemented for now

        //register action
        //have logic about the action
        action.ApplyEffect();
    }
}
