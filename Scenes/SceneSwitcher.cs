using Godot;
using System.Collections.Generic;

public partial class SceneSwitcher : Node
{

	public Stack<Node> sceneStack = new Stack<Node>();

	public static Node root;

	public static GameLoop CurrentGameLoop = null;

	public static SceneSwitcher Instance = null;

	public override void _Ready()
	{
		root = GetTree().Root;
		PushScene(GD.Load<PackedScene>("res://Scenes/menus/MainMenu.tscn").Instantiate<Node>());
		SettingsManager.GetInstance();
		PlayerStatsManager.GetInstance();
        CardLoadingManager.GetInstance();
		DeckManager.GetInstance();
		Instance = this;
    }

	public void PushScene(Node node)
	{

		
		if (sceneStack.Count > 0)
		{
			this.RemoveChild(sceneStack.Peek());
		}
		
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
