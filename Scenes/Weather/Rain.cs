using Godot;
using System;

public partial class Rain : Weather
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        this.weatherType = WeatherType.Rain;
        this.damageAmount = 1;
        this.damageInterval = 0.1f;
        this.durationSeconds = 60;
		this.statusEffectsToApply.Add(GD.Load<PackedScene>("res://Scenes/StatusEffects/WetEffect.tscn"));
		this.damageType = DamageType.Water;
        base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}
}
