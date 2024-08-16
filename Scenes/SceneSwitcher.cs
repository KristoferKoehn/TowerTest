using Godot;
using System.Collections.Generic;

public partial class SceneSwitcher : Node
{
    [Signal]
    public delegate void LoadingBarAnimationFinishedEventHandler();



    private bool loading = false;
    private string _loadScreenPath = "res://Scenes/Utility/Autoloads/loading_screen.tscn";
    private LoadingScreen _loadScreen;
    private PackedScene _loadedResource;
    private Godot.Collections.Array _progress = new Godot.Collections.Array();
    private bool _useSubThreads = false; // if true, uses multiple threads to load resource
    private string _scenePath;

    public Stack<Node> sceneStack = new Stack<Node>();
    public static Node root;
    public static GameLoop CurrentGameLoop = null;
    public static SceneSwitcher Instance = null;

    public override void _Ready()
    {
        root = GetTree().Root;
        SettingsManager.GetInstance();
        PlayerStatsManager.GetInstance();
        CardLoadingManager.GetInstance();
        DeckManager.GetInstance();
        Instance = this;
        PushScene("res://Scenes/menus/MainMenu.tscn");
    }

    public void PushScene(string _scenePath)
    {
        Node topnode = null;
        if (sceneStack.Count > 0)
        {
            topnode = sceneStack.Peek();
        }
        this.LoadScene(_scenePath);

        LoadingBarAnimationFinished += () =>
        {
            if (topnode != null)
            {
                GD.Print($"Removing node {topnode.Name}");
                this.RemoveChild(topnode);
            }
        };


    }

    public void PopScene()
    {
        Node node = sceneStack.Pop();
        this.RemoveChild(node);
        node.QueueFree();
        this.AddChild(sceneStack.Peek());
    }

    private void LoadScene(string _scenePath)
    {
        this._scenePath = _scenePath;
        _loadScreen = GD.Load<PackedScene>(_loadScreenPath).Instantiate<LoadingScreen>();
        this.GetTree().Root.CallDeferred("add_child", _loadScreen);
        _loadScreen._animationPlayer.AnimationFinished += (animation) =>
        {
            if (animation == "start_load")
            {
                EmitSignal("LoadingBarAnimationFinished");
            }

            var state = ResourceLoader.LoadThreadedRequest(_scenePath, "", _useSubThreads);
            if (state == Error.Ok)
            {
                loading = true;
            }

        };

    }

    private void DoLoadingLogic()
    {
        if(loading)
        {
            ResourceLoader.ThreadLoadStatus loadStatus = ResourceLoader.LoadThreadedGetStatus(_scenePath, _progress);
            switch ((int)loadStatus)
            {
                case 0: // THREAD_LOAD_INVALID_RESOURCE
                case 2: // THREAD_LOAD_FAILED
                    loading = false;
                    return;
                case 1: // THREAD_LOAD_IN_PROGRESS
                    this._loadScreen.UpdateProgressBar((float)_progress[0]);
                    break;
                case 3: // THREAD_LOAD_LOADED
                    _loadedResource = (PackedScene)ResourceLoader.LoadThreadedGet(_scenePath);
                    Node node = _loadedResource.Instantiate();
                    sceneStack.Push(node);
                    CallDeferred("add_child", node); //this.AddChild(node);
                    this._loadScreen._progressBar.Value = 100;
                    this._loadScreen.StartOutroAnimation();
                    loading = false;
                    break;
            }
        }
    }

    public override void _Process(double delta)
    {
        DoLoadingLogic();
    }
}
