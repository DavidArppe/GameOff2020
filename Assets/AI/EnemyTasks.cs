using Bonsai;
using Bonsai.Core;
using UnityEngine;

[BonsaiNode("Tasks/EnemyActions/", "Wander")]
public class Wander : Task
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

[BonsaiNode("Tasks/EnemyActions/", "Hide")]
public class Hide : Task
{
    public override void OnEnter()
    {
        Actor.SendMessage("SwitchToHideMode");
    }

    public override Status Run()
    {
        return Status.Success;
    }
}

[BonsaiNode("Tasks/EnemyActions/", "Stop")]
public class Stop : Task
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