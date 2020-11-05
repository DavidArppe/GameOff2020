using System;
using UnityEngine;

public abstract class PivotBasedCameraRig : AbstractTargetFollower
{
    protected Transform cam; // the transform of the camera
    protected Transform pivot; // the point at which the camera pivots around
    protected Vector3 lastTargetPosition;

    protected virtual void Awake()
    {
        // find the camera in the object hierarchy
        cam = GetComponentInChildren<Camera>().transform;
        pivot = cam.parent;
    }
}
