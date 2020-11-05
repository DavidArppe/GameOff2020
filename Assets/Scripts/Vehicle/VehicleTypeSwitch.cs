using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RVP.VehicleParent), typeof(RVP.VehicleAssist), typeof(Rigidbody))]
public class VehicleTypeSwitch : MonoBehaviour
{
    [Flags]
    public enum VehicleType
    {
        NONE = 0,
        JET = 1,
        HOVER = 2,
        CAR = 4,
    }

    public VehicleType vehicleType = VehicleType.JET;
    
    [Header("Required Variables")]
    public GameObject               hoverWheels;
    public GameObject               regularWheels;

    public Transform[]              regularWheelTransforms;
    public Transform[]              hoverWheelTransforms;
    public Transform[]              jetWheelTransforms;
    public Transform[]              visualWheelTransforms;
    
    private new Rigidbody           rigidbody;
    private RVP.VehicleParent       vehicleParent;
    private RVP.VehicleAssist       vehicleAssist;

    private RVP.GasMotor            gasMotor;
    private RVP.HoverTankMotor      hoverMotor;
    private RVP.SteeringControl     regularSteer;
    private RVP.HoverTankSteer      hoverSteer;
    private RVP.Transmission        transmission;
    private RVP.TireScreech         tireScreech;
    private JetMotorAndController   jetController;

    public Transform               leftWing;
    public Transform               rightWing;

    [Header("Tweakable Variables")]
    public float        hoverCenterOfGravity = -0.3f;
    public float        regularCenterOfGravity = -0.1f;

    public float        jetHoverDistance = 20.0f;
    public float        hovercraftHoverDistance = 5.0f;

    // Linear interpolation values
    private float       isHoverLerpTarget = 1.0f;
    private float       isHoverLerpValue = 1.0f;

    private float       isJetLerpTarget = 1.0f;
    private float       isJetLerpValue = 1.0f;
    public const float jetAnimationStartVel    = 15.0f;
    public const float jetAnimationEndVel      = 35.0f;

    private float originalDrag;
    private float originalAngularDrag;

    private void Start()
    {
        // Initialize the variables we can initialize like this
        rigidbody       = gameObject.GetComponent<Rigidbody>();
        vehicleParent   = gameObject.GetComponent<RVP.VehicleParent>();
        vehicleAssist   = gameObject.GetComponent<RVP.VehicleAssist>();
        gasMotor        = gameObject.GetComponentInChildren<RVP.GasMotor>();
        hoverMotor      = gameObject.GetComponentInChildren<RVP.HoverTankMotor>();
        regularSteer    = gameObject.GetComponentInChildren<RVP.SteeringControl>();
        hoverSteer      = gameObject.GetComponentInChildren<RVP.HoverTankSteer>();
        transmission    = gameObject.GetComponentInChildren<RVP.Transmission>();
        tireScreech     = gameObject.GetComponentInChildren<RVP.TireScreech>();
        jetController   = gameObject.GetComponentInChildren<JetMotorAndController>();

        originalDrag = rigidbody.drag;
        originalAngularDrag = rigidbody.angularDrag;

        // Setup callbacks for the controls
        UnityInputModule.instance.controls.Player.Switch.canceled += context => { OnSwitch(); };
        UnityInputModule.instance.controls.Player.Switch.performed += context =>
        {
            ToggleVehicleBit(VehicleType.JET);
            OnSwitch(false, HasVehicleBit(VehicleType.JET) ? VehicleType.HOVER : VehicleType.NONE);
            jetController.gameObject.SetActive(HasVehicleBit(VehicleType.JET));
            isJetLerpTarget = HasVehicleBit(VehicleType.JET) ? 1.0f : 0.0f;
            rigidbody.drag = HasVehicleBit(VehicleType.JET) ? jetController.jetDrag : originalDrag;
            rigidbody.angularDrag = HasVehicleBit(VehicleType.JET) ? jetController.jetAngularDrag : originalAngularDrag;
            hoverSteer.ToggleJetHover(HasVehicleBit(VehicleType.JET));
        };

        // If both Hover and Car have their bits matching, then toggle one. We can only initialize with one enabled. They are mutually exclusive
        if (HasVehicleBit(VehicleType.HOVER) == HasVehicleBit(VehicleType.CAR)) 
        {
            ToggleVehicleBit(VehicleType.HOVER);
        }

        // Don't switch, just initialize. Force hover initialize if it's a jet.
        OnSwitch(false, HasVehicleBit(VehicleType.JET) ? VehicleType.HOVER : VehicleType.NONE);

        jetController.gameObject.SetActive(HasVehicleBit(VehicleType.JET));

        isJetLerpTarget = HasVehicleBit(VehicleType.JET) ? 1.0f : 0.0f;
        isHoverLerpTarget = HasVehicleBit(VehicleType.HOVER) ? 1.0f : 0.0f;
        rigidbody.drag = HasVehicleBit(VehicleType.JET) ? jetController.jetDrag : originalDrag;
        rigidbody.angularDrag = HasVehicleBit(VehicleType.JET) ? jetController.jetAngularDrag : originalAngularDrag;
    }

    private void LateUpdate()
    {
        // If it's a jet, or a hovercraft, the wheels should by default target hovercraft visuals
        isHoverLerpValue = Mathf.MoveTowards(isHoverLerpValue, Mathf.Max(isJetLerpTarget, isHoverLerpTarget), Time.deltaTime * 3.0f);
        isJetLerpValue = Mathf.MoveTowards(isJetLerpValue, isJetLerpTarget, Time.deltaTime * 2.0f);

        var relativeVelocity = transform.InverseTransformDirection(rigidbody.velocity);

        var jetAnimate01 = Utilities.ActualSmoothstep(jetAnimationStartVel, jetAnimationEndVel, relativeVelocity.z);

        // Interpolate to the jet wheels, only if it's a jet AND the speed is over a threshold
        float realHoverLerp = Mathf.SmoothStep(0.0f, 1.0f, isHoverLerpValue);
        float realJetWheelLerp = Mathf.SmoothStep(0.0f, 1.0f, Mathf.Min(jetAnimate01, isJetLerpValue));
        for (int i = 0; i < visualWheelTransforms.Length; i++)
        {
            var carToHoverWheelPos = Vector3.Lerp(regularWheelTransforms[i].position, hoverWheelTransforms[i].position, realHoverLerp);
            var carToHoverWheelRot = Quaternion.Lerp(regularWheelTransforms[i].rotation, hoverWheelTransforms[i].rotation, realHoverLerp);

            var toJetWheelPos = Vector3.Lerp(carToHoverWheelPos, jetWheelTransforms[i].position, realJetWheelLerp);
            var toJetWheelRot = Quaternion.Lerp(carToHoverWheelRot, jetWheelTransforms[i].rotation, realJetWheelLerp);

            visualWheelTransforms[i].position = toJetWheelPos;
            visualWheelTransforms[i].rotation = toJetWheelRot;
        }

        // TODO: Use the animator for this? Makes it more expandable
        leftWing.localPosition = Vector3.Lerp(Vector3.right * 4.0f, Vector3.zero, isJetLerpValue);
        rightWing.localPosition = Vector3.Lerp(Vector3.right * -4.0f, Vector3.zero, isJetLerpValue);

        leftWing.localScale = Vector3.Lerp(new Vector3(0.5f, 1.0f, 1.0f), Vector3.one, isJetLerpValue);
        rightWing.localScale = Vector3.Lerp(new Vector3(0.5f, 1.0f, 1.0f), Vector3.one, isJetLerpValue);
    }

    bool HasVehicleBit(VehicleType typeToCheck)
    {
        return (vehicleType & typeToCheck) == typeToCheck;
    }

    void ToggleVehicleBit(VehicleType typeToToggle)
    {
        vehicleType ^= typeToToggle;
    }

    void OnSwitch(bool doToggle = true, VehicleType forceSwitchType = VehicleType.NONE)
    {
        // Exit early if you are a jet. Need to hold to switch to a ground-type first
        if (HasVehicleBit(VehicleType.JET) && forceSwitchType == VehicleType.NONE) return;

        if (doToggle && forceSwitchType == VehicleType.NONE)
        {
            // Swap the bits for the hovercar and the car. Works only if they start staggered, which is enforced
            ToggleVehicleBit(VehicleType.HOVER);    // Toggle the Hover bitmask
            ToggleVehicleBit(VehicleType.CAR);      // Toggle the Car bitmask
        }
        else if (doToggle && forceSwitchType != VehicleType.NONE)
        {
            if (forceSwitchType == VehicleType.HOVER && !HasVehicleBit(VehicleType.HOVER)) ToggleVehicleBit(VehicleType.HOVER);
            else if (forceSwitchType != VehicleType.HOVER && HasVehicleBit(VehicleType.HOVER)) ToggleVehicleBit(VehicleType.HOVER);

            if (forceSwitchType == VehicleType.CAR && !HasVehicleBit(VehicleType.CAR)) ToggleVehicleBit(VehicleType.CAR);
            else if (forceSwitchType != VehicleType.CAR && HasVehicleBit(VehicleType.CAR)) ToggleVehicleBit(VehicleType.CAR);
        }

        // Set the vehicleParent hover variable
        vehicleParent.hover = forceSwitchType != VehicleType.NONE ?
            (forceSwitchType == VehicleType.HOVER ? true : false) :
            HasVehicleBit(VehicleType.HOVER);

        isHoverLerpTarget = vehicleParent.hover ? 1.0f : 0.0f;

        ClearVehicleSettings();

        if (vehicleParent.hover)    EnableHovercraftSettings();
        else                        EnableCarSettings();
    }

    void ClearVehicleSettings()
    {
        // Disable hover valeus
        hoverWheels.SetActive(false);
        hoverMotor.gameObject.SetActive(false);
        hoverSteer.gameObject.SetActive(false);

        // Disable car values
        regularWheels.SetActive(false);
        gasMotor.gameObject.SetActive(false);
        tireScreech.gameObject.SetActive(false);
        regularSteer.gameObject.SetActive(false);
        transmission.gameObject.SetActive(false);
    }

    void EnableHovercraftSettings()
    {
        hoverWheels.SetActive(true);
        hoverMotor.gameObject.SetActive(true);
        hoverSteer.gameObject.SetActive(true);
        hoverSteer.ToggleJetHover(HasVehicleBit(VehicleType.JET));

        if (HasVehicleBit(VehicleType.JET))
        {
            foreach (var wheel in hoverWheels.GetComponentsInChildren<RVP.HoverTankWheel>()) wheel.hoverDistance = jetHoverDistance;
        }
        else
        {
            foreach (var wheel in hoverWheels.GetComponentsInChildren<RVP.HoverTankWheel>()) wheel.hoverDistance = hovercraftHoverDistance;
        }

        vehicleParent.engine                = hoverMotor;
        vehicleAssist.enabled               = false;
        vehicleParent.burnoutSpin           = 0;
        vehicleParent.holdEbrakePark        = false;
        vehicleParent.burnoutThreshold      = -1;
        vehicleParent.centerOfMassOffset    = Vector3.up * hoverCenterOfGravity;

        vehicleParent.SetCenterOfMass();
    }
    
    void EnableCarSettings()
    {
        regularWheels.SetActive(true);
        gasMotor.gameObject.SetActive(true);
        tireScreech.gameObject.SetActive(true);
        regularSteer.gameObject.SetActive(true);
        transmission.gameObject.SetActive(true);
        
        vehicleParent.engine                = gasMotor;
        vehicleAssist.enabled               = true;
        vehicleParent.burnoutSpin           = 5;
        vehicleParent.holdEbrakePark        = true;
        vehicleParent.burnoutThreshold      = 0.9f;
        vehicleParent.centerOfMassOffset    = Vector3.up * -regularCenterOfGravity;

        vehicleParent.SetCenterOfMass();
    }
}
