using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleTypeSwitch : MonoBehaviour
{
    public RVP.GasMotor             gasMotor;
    public RVP.HoverMotor           hoverMotor;

    public RVP.SteeringControl      regularSteer;
    public RVP.HoverSteer           hoverSteer;
    public GameObject               hoverWheels;
    public GameObject               regularWheels;

    public RVP.Transmission         transmission;
    public RVP.TireScreech          tireScreech;

    public RVP.VehicleParent        vehicleParent;

    public float hoverCenterOfGravity = -0.3f;
    public float regularCenterOfGravity = -0.1f;

    // Update is called once per frame
    void Update()
    {
        if (UnityInputModule.instance.controls.Player.Switch.ReadValue<float>() > 0.5f)
        {
            vehicleParent.hover = !vehicleParent.hover;
            
            if (vehicleParent.hover)
            {
                vehicleParent.engine = hoverMotor;

                hoverWheels.SetActive(true);
                hoverMotor.gameObject.SetActive(true);
                hoverSteer.gameObject.SetActive(true);

                regularWheels.SetActive(false);
                gasMotor.gameObject.SetActive(false);
                regularSteer.gameObject.SetActive(false);
                transmission.gameObject.SetActive(false);
                tireScreech.gameObject.SetActive(false);

                vehicleParent.burnoutThreshold = -1;
                vehicleParent.burnoutSpin = 0;
                vehicleParent.holdEbrakePark = false;

                vehicleParent.centerOfMassOffset = Vector3.up * -hoverCenterOfGravity;
            }
            else
            {
                vehicleParent.engine = gasMotor;

                hoverWheels.SetActive(false);
                hoverMotor.gameObject.SetActive(false);
                hoverSteer.gameObject.SetActive(false);

                regularWheels.SetActive(true);
                gasMotor.gameObject.SetActive(true);
                regularSteer.gameObject.SetActive(true);
                transmission.gameObject.SetActive(true);
                tireScreech.gameObject.SetActive(true);

                vehicleParent.burnoutThreshold = 0.9f;
                vehicleParent.burnoutSpin = 5;
                vehicleParent.holdEbrakePark = true;

                vehicleParent.centerOfMassOffset = Vector3.up * -regularCenterOfGravity;
            }
        }
    }
}
