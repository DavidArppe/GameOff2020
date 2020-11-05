using UnityEngine;
using UnityEngine.InputSystem;
using RVP;

//Class for setting the input with the input manager
[DisallowMultipleComponent]
public class UnityInputModule : MonoBehaviour
{
    private static UnityInputModule _inputModule;
    public static UnityInputModule instance
    {
        get
        {
            if (_inputModule == null)
            {
                _inputModule = FindObjectOfType<UnityInputModule>();
            }
            return _inputModule;
        }
    }

    [HideInInspector]
    public ControlActions controls
    {
        get
        {
            if (_controls == null)
            {
#if !UNITY_EDITOR
                HoldingInteraction.Initialize();
#endif
                _controls = new ControlActions();
            }
            return _controls;
        }
    }

    private ControlActions _controls = null;

    public VehicleParent vehicleParent
    {
        get
        {
            if (_vehicleParent == null)
            {
                _vehicleParent = FindObjectOfType<VehicleParent>();
            }
            return _vehicleParent;
        }
    }
    private VehicleParent _vehicleParent;

    private CameraControl cam;

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    void FixedUpdate()
    {
        if (vehicleParent)
        {
            //Get constant inputs
            vehicleParent.SetAccel(controls.Player.Accelerate.ReadValue<float>());
            vehicleParent.SetBrake(controls.Player.Break.ReadValue<float>());
            vehicleParent.SetSteer(controls.Player.Steer.ReadValue<float>());
            vehicleParent.SetEbrake(controls.Player.Ebreak.ReadValue<float>());
            vehicleParent.SetBoost(controls.Player.Boost.ReadValue<float>() > 0);
            vehicleParent.SetPitch(controls.Player.Pitch.ReadValue<float>());
            vehicleParent.SetYaw(controls.Player.Yaw.ReadValue<float>());
            vehicleParent.SetRoll(controls.Player.Roll.ReadValue<float>());
            vehicleParent.SetRoll(controls.Player.Roll.ReadValue<float>());
        }
    }
}