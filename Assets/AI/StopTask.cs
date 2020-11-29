using Bonsai;
using Bonsai.Core;
using UnityEngine;

[BonsaiNode("Tasks/EnemyActions/", "Stop")]
public class StopTask : Task
{
    public override void OnEnter()
    {
        Actor.SendMessage("StopMovement");
    }

    public override Status Run()
    {
        return Status.Success;
    }
}