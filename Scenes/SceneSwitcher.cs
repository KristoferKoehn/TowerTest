using Godot;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;
using System.Threading;

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
        // We have two types of singletons: metaprogression (account stuff), and then ones specific to a run.
        root = GetTree().Root;
        SettingsManager.GetInstance();
        CardLoadingManager.GetInstance();
        DeckManager.GetInstance();
        AccountStatsManager.GetInstance();
        Instance = this;
        PushScene("res://Scenes/menus/MainMenu.tscn");
    }

    public void PushScene(string _scenePath) // used to move to another scene
    {
        Node previousScene = null;
        if (sceneStack.Count > 0)
        {
            previousScene = sceneStack.Peek();
        }

        this.LoadScene(_scenePath);

        LoadingBarAnimationFinished += () =>
        {
            if (previousScene != null && previousScene.GetParent() == this)
            {
                this.RemoveChild(previousScene); // Only remove if it is still a child
            }
        };
    }


    public void PopScene() // used to go back to the previous scene (gets rid of the current scene forever).
    {
        if (sceneStack.Count == 0)
        {
            return;
        }

        Node node = sceneStack.Pop();

        if (node.GetParent() == this)
        {
            this.RemoveChild(node);
            node.QueueFree();
        }

        if (sceneStack.Count > 0)
        {
            Node previousScene = sceneStack.Peek();
            if (previousScene.GetParent() != this)
            {
                this.AddChild(previousScene);
            }
        }
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
        if (!loading) return;

        ResourceLoader.ThreadLoadStatus loadStatus = ResourceLoader.LoadThreadedGetStatus(_scenePath, _progress);
        switch (loadStatus)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource: // THREAD_LOAD_INVALID_RESOURCE
            case ResourceLoader.ThreadLoadStatus.Failed: // THREAD_LOAD_FAILED
                loading = false;
                return;
            case ResourceLoader.ThreadLoadStatus.InProgress: // THREAD_LOAD_IN_PROGRESS
                this._loadScreen.UpdateProgressBar((float)_progress[0]);
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded: // THREAD_LOAD_LOADED
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

    public override void _Process(double delta)
    {
        DoLoadingLogic();
    }
}
