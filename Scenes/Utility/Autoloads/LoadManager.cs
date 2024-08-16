using Godot;
using System;
using System.Collections.Generic;

public partial class LoadManager : Node
{
    [Signal]
    public delegate void ProgressChangedEventHandler(float progress);

    [Signal]
    public delegate void LoadDoneEventHandler();

    private string _loadScreenPath = "res://Scenes/Utility/Autoloads/loading_screen.tscn";
    private PackedScene _loadScreen;
    private PackedScene _loadedResource;
    private string _scenePath;
    private Godot.Collections.Array _progress = new Godot.Collections.Array();
    private bool _useSubThreads = false; // if true, uses multiple threads to load resource

    public override void _Ready()
    {
        _loadScreen = (PackedScene)ResourceLoader.Load(_loadScreenPath);
        SetProcess(false);
    }

    public void LoadScene(string scenePath)
    {
        _scenePath = scenePath; // Set global scene path to the one passed to this function
        LoadingScreen newLoadingScreen = _loadScreen.Instantiate<LoadingScreen>();
        GetTree().Root.AddChild(newLoadingScreen);

        // Connect signals
        //Connect("progress_changed", newLoadingScreen, "_update_progress_bar");
        this.ProgressChanged += newLoadingScreen.UpdateProgressBar;
        //Connect("load_done", newLoadingScreen, "_start_outro_animation");
        this.LoadDone += newLoadingScreen.StartOutroAnimation;

        // Await loading screen full coverage
        //newLoadingScreen.Connect("loading_screen_has_full_coverage", this, nameof(OnLoadingScreenHasFullCoverage));
        newLoadingScreen.LoadingScreenHasFullCoverage += OnLoadingScreenHasFullCoverage;
    }

    private async void OnLoadingScreenHasFullCoverage()
    {
        StartLoad();
        await ToSignal(this, "progress_changed"); // Wait for progress update
    }

    private void StartLoad()
    {
        var state = ResourceLoader.LoadThreadedRequest(_scenePath, "", _useSubThreads);
        if (state == Error.Ok)
        {
            SetProcess(true);
        }
    }

    public override void _Process(double delta)
    {
        ResourceLoader.ThreadLoadStatus loadStatus = ResourceLoader.LoadThreadedGetStatus(_scenePath, _progress);
        switch ((int)loadStatus)
        {
            case 0: // THREAD_LOAD_INVALID_RESOURCE
            case 2: // THREAD_LOAD_FAILED
                SetProcess(false);
                return;
            case 1: // THREAD_LOAD_IN_PROGRESS
                EmitSignal("progress_changed", _progress[0]);
                break;
            case 3: // THREAD_LOAD_LOADED
                _loadedResource = (PackedScene)ResourceLoader.LoadThreadedGet(_scenePath);
                EmitSignal("progress_changed", 1.0f);
                EmitSignal("load_done");
                GetTree().ChangeSceneToPacked(_loadedResource);
                SetProcess(false);
                break;
        }
    }
}
