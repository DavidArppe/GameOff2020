using System;
using UnityEngine;

#pragma warning disable 649
public abstract class AbstractTargetFollower : MonoBehaviour
{
    public enum UpdateType // The available methods of updating are:
    {
        FixedUpdate, // Update in FixedUpdate (for tracking rigidbodies).
        ManualUpdate, // user must call to update camera
    }

    [SerializeField] protected Transform target;            // The target object to follow
    [SerializeField] private UpdateType updateType;         // stores the selected update type

    protected Rigidbody targetRigidbody;

    private void FixedUpdate()
    {
        if (updateType == UpdateType.FixedUpdate)
        {
            FollowTarget(Time.deltaTime);
        }
    }

    public void ManualUpdate()
    {
        if (updateType == UpdateType.ManualUpdate)
        {
            FollowTarget(Time.deltaTime);
        }
    }

    protected abstract void FollowTarget(float deltaTime);

    public virtual void SetTarget(Transform newTransform)
    {
        target = newTransform;
    }


    public Transform Target
    {
        get { return target; }
    }
}
