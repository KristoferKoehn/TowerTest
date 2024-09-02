using Godot;
using Godot.Collections;
using System;

public partial class PredictionManager : Node
{

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

    public override void _Ready()
    {

    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    /// <summary>
    /// predicts the position of the target along the path3d in 'seconds' seconds
    /// </summary>
    /// <param name="target">target who's position is predicted</param>
    /// <param name="seconds">amount of time ahead of target is predicted</param>
    /// <returns>Vector3 in global space of where the enemy will be after seconds</returns>
	public Vector3 PredictTarget(BaseEnemy target, float seconds)
	{

        float Distance = target.StatBlock.GetStat(StatType.Speed) * seconds;
        float currentOffset = target.Progress;
        Path3D currentPath = target.GetParent<Path3D>();

        
        while (Distance > 0)
        {
            if (currentPath.Curve.GetBakedLength() - currentOffset < Distance)
            {
                Distance -= currentPath.Curve.GetBakedLength() - currentOffset;

                Random r = new Random();
                Path3D nextPath = GetNextPath(target, r.Next());

                if (nextPath == null)
                {
                    //no solution (end of path)
                    GD.Print("END OF PATH NO SOLUTION");
                    currentOffset = currentPath.Curve.GetBakedLength();
                    break;
                }

                currentPath = nextPath;
                currentOffset = 0;
            }
            else if (currentPath.Curve.GetBakedLength() - currentOffset > Distance)
            {
                currentOffset += Distance;
                break;
            }
        }

        Vector3 pos = currentPath.Curve.SampleBaked(currentOffset);
        return currentPath.GlobalTransform * pos;
	}

    public Path3D GetNextPath(BaseEnemy target, int idx)
    {

        Array<Path3D> paths = null;
        
        Chunk chunk = target.GetParent().GetParent() as Chunk;
        Spawner spawner = target.GetParent() as Spawner;
        if (chunk != null && spawner == null)
        {
            paths = chunk.NextPaths;
        }
        else if (spawner != null)
        {
            paths = spawner.NextPaths;
        }

        if (paths != null && paths.Count > 0)
        {
            return paths[idx % paths.Count];
        }
        else
        {
            return null;
        }
    }

}
