using Godot;
using System;

public partial class LoadingScreen : CanvasLayer
{
    [Export] public AnimationPlayer _animationPlayer;
    [Export] public ProgressBar _progressBar;

    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _progressBar = GetNode<ProgressBar>("Panel/ProgressBar");
        //this._progressBar.AddThemeStyleboxOverride("rand_color", new StyleBoxFlat());
    }

    public void UpdateProgressBar(float newValue)
    {
        _progressBar.Value = newValue * 100;
    }

    public void StartOutroAnimation()
    {
        _animationPlayer.Play("end_load");
    }
}

/* GDscript code:
extends CanvasLayer

#signal loading_screen_has_full_coverage

@onready var animationPlayer : AnimationPlayer = $AnimationPlayer
@onready var progressBar : ProgressBar= $Panel/ProgressBar

func _update_progress_bar(new_value : float) -> void:
	progressBar.value = (new_value * 100)
	
func _start_outro_animation() -> void:
	await Signal(animationPlayer, "animation_finished")
	animationPlayer.play("end_load")
	await Signal(animationPlayer, "animation_finished")
	self.queue_free()

*/
