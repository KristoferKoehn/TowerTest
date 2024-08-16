using Godot;
using System;

public partial class LoadingScreen : CanvasLayer
{
    [Signal]
    public delegate void LoadingScreenHasFullCoverageEventHandler();

    private AnimationPlayer _animationPlayer;
    private ProgressBar _progressBar;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _progressBar = GetNode<ProgressBar>("Panel/ProgressBar");
    }

    public void UpdateProgressBar(float newValue)
    {
        _progressBar.Value = newValue * 100;
    }

    public async void StartOutroAnimation()
    {
        // Wait for the animation to finish
        await ToSignal(_animationPlayer, "animation_finished");
        _animationPlayer.Play("end_load");
        await ToSignal(_animationPlayer, "animation_finished");
        QueueFree();
    }
}
