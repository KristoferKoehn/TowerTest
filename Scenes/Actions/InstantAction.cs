using Godot;
using TowerTest.Scenes.Components;

public partial class InstantAction : AbstractPlaceable
{
    [Export] StatType Stat { get; set; }
    [Export] float Value { get; set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public override void DisplayMode()
    {
        //do nothing
    }

    public override void ActivatePlacing()
    {

        GD.Print("Instant in placing mode");
        ActionManager.GetInstance().AddAction(this);

        //this does stat change
    }

    public void ApplyEffect()
    {
        GD.Print("placing the instant card");
        EmitSignal("Placed",this, Vector3.Zero);
        AccountStatsManager.GetInstance().ChangeStat(Stat, Value);
        PlayerStatsManager.GetInstance().StatBlock.SetStat(Stat, PlayerStatsManager.GetInstance().StatBlock.GetStat(Stat) + Value);
    }
}
