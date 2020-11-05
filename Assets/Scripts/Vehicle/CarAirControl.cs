using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVP;

public class CarAirControl : MonoBehaviour
{
    public VehicleParent vehicleParent;

    public float pitchPower = 10.0f;
    public float rollPower = 10.0f;

    private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = vehicleParent.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float roll = UnityInputModule.instance.controls.Player.TankStrafe.ReadValue<float>();
        float pitch = UnityInputModule.instance.controls.Player.TankAccelerate.ReadValue<float>();

        rigidbody.AddRelativeTorque(new Vector3(pitch * pitchPower, 0.0f, -roll * rollPower));
    }
}