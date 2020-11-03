
using System.Collections.Generic;
using UnityEngine;

public interface IEntity : ITeamFiltered
{
    bool IsDead { get; }
    float Health { get; }
    float Shield { get; }
    IList<IWeapon> ActiveWeapons { get; }
    Rigidbody RigidBody { get; }
}