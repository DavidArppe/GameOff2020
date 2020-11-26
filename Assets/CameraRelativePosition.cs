using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRelativePosition : MonoBehaviour
{
    public Camera cameraToFollow;
    public float localZRotationAmount = -10.5f;

    private Vector3 offsetToCamera;

    void Start()
    {
        transform.LookAt(cameraToFollow.transform);
        transform.forward *= -1.0f;
        transform.Rotate(Vector3.forward, localZRotationAmount, Space.Self);

        offsetToCamera = transform.position - cameraToFollow.transform.position;
        offsetToCamera = offsetToCamera.normalized * 9000.0f;
    }

    void Update()
    {
        transform.position = cameraToFollow.transform.position + offsetToCamera;
    }
}
