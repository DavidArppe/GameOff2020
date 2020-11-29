using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bonsai;
using Bonsai.Core;

[BonsaiNode("Conditional/PlayerConditions/", "PlayerNear")]
public class PlayerNear : Task
{
    public float playerProximityRadius;

    public override Status Run()
    {
        return ((Actor.transform.position - GlobalControl.playerStatic.transform.position).magnitude < playerProximityRadius) ? Status.Success : Status.Running;
    }
}
