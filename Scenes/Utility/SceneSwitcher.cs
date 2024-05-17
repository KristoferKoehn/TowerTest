using Godot;
using System;
using System.Collections.Generic;

public partial class SceneSwitcher : Node
{

    public Stack<Node> sceneStack = new Stack<Node>();

    static Node root;



    public override void _Ready()
    {
        root = GetTree().Root;
        PushScene(GD.Load<PackedScene>("res://Scenes/menus/MainMenu.tscn").Instantiate<Node>());
    }

    public void PushScene(Node node)
    {
        this.sceneStack.Push(node);
        this.AddChild(node);
    }

    public void PopScene()
    {
        Node node = sceneStack.Pop();
        this.RemoveChild(node);
        node.QueueFree();
        this.AddChild(sceneStack.Peek());
    }

}
