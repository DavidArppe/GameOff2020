using Bonsai;
using Bonsai.Core;
using UnityEngine;

[BonsaiNode("Tasks/EnemyActions/", "Wander")]
public class WanderTask : Task
{
    public override void OnEnter()
    {
        Actor.SendMessage("SwitchToWanderMode");
    }

    public override Status Run()
    {
        return Status.Success;
    }
}