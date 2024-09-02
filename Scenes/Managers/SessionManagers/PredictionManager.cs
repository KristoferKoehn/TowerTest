using Godot;
using Godot.Collections;
using System;

public partial class PredictionManager : Node
{

    static PredictionManager instance;
    MeshInstance3D meshInstance;


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
        meshInstance = new MeshInstance3D();
        this.AddChild(meshInstance);
        meshInstance.Mesh = new SphereMesh();
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

                Path3D nextPath = GetNextPath(target);

                if (nextPath == null)
                {

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
        meshInstance.GlobalPosition = currentPath.GlobalTransform * pos;
        return currentPath.GlobalTransform * pos;
	}

    public Path3D GetNextPath(BaseEnemy target)
    {

        Array<Path3D> paths = null;
        Array<int> ForkDecisions = target.ForkDecisions;
        Chunk chunk = target.GetParent().GetParent() as Chunk;
        int chunkDistance = -1;
        Spawner spawner = target.GetParent() as Spawner;
        if (chunk != null && spawner == null)
        {
            paths = chunk.NextPaths;
            chunkDistance = chunk.ChunkDistance;
        }
        else if (spawner != null)
        {
            paths = spawner.NextPaths;
            chunkDistance = int.MaxValue;

        }



        if (paths != null && paths.Count > 0)
        {
            return paths[ForkDecisions[chunkDistance % 64] % paths.Count];
        }
        else
        {
            return null;
        }
    }

}
