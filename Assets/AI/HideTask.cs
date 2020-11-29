using Bonsai;
using Bonsai.Core;
using UnityEngine;

[BonsaiNode("Tasks/EnemyActions/", "Hide")]
public class HideTask : Task
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