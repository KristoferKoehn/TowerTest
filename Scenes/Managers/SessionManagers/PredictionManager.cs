using Godot;

public partial class PredictionManager : Node
{

	PackedScene PackedScene = GD.Load<PackedScene>("res://Scenes/UI/TargetPrediction.tscn");
	TargetPrediction TargetPrediction = null;

    static PredictionManager instance;


    public static PredictionManager GetInstance()
    {
        if (!IsInstanceValid(instance))
        {
            instance = new PredictionManager();
            SceneSwitcher.root.GetNode<GameLoop>("SceneSwitcher/GameLoop").AddChild(instance);
            instance.Name = "PredictionManager";
        }
        return instance;
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

	public Vector3 PredictTarget(BaseEnemy target, float seconds)
	{
        if (TargetPrediction == null)
        {
            TargetPrediction = PackedScene.Instantiate<TargetPrediction>();
            target.GetParent<Path3D>().AddChild(TargetPrediction);
        } else {
            TargetPrediction.Reparent(target.GetParent<Path3D>());
        }

        TargetPrediction.disabled = false;

        GD.Print(TargetPrediction.GetParent().Name);
        TargetPrediction.Progress = target.Progress;

        return TargetPrediction.GetPositionIn(seconds, target.StatBlock.GetStat(StatType.Speed));
	}

}
