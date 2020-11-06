using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVP;

public class JetMotorAndController : MonoBehaviour
{
    public VehicleParent vehicleParent;

    private new Rigidbody rigidbody;

    public Vector3 turnTorques      = new Vector3(60.0f, 10.0f, 90.0f);
    public float dragIncreaseFactor = 0.001f; 
    public float hoverFlipOverForce = 50.0f;
    public float aerodynamicEffect  = 0.02f;  
    public float airBrakesEffect    = 3f;     
    public float jetAngularDrag     = 5.0f;
    public float acceleration       = 10.0f;
    public float bankTorque         = 5.0f;
    public float maxThrust          = 3000.0f;
    public float brakeDrag          = 5.0f;
    public float topSpeed           = 200.0f;
    public float jetDrag            = 0.1f;
    [Range(0.0f, 1.0f)]
    public float idleSpeedInput     = 0.5f;

    private const float FORCE_MULT      = 100.0f;
    [HideInInspector]
    public float targetThrottleValue   = 0.0f;
    private float throttleSpeedUpTime   = 4.0f;
    private float throttleValue         = 0.0f;

    private float originalAngularDrag;  
    private float originalDrag;         
    private float aeroFactor;

    /// <summary>
    /// Get the rigidbody from the vehicle parent
    /// </summary>
    private void Awake()
    {
        rigidbody = vehicleParent.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Initialize variables and store original drag variables
    /// </summary>
    private void Start()
    {
        originalAngularDrag = rigidbody.angularDrag;
        originalDrag = rigidbody.drag;
    }

    /// <summary>
    /// Gather input, and call the move function. Also, add a stabilization force if the speed is low enough (to keep the ship
    /// upright while hovering)
    /// </summary>
    private void FixedUpdate()
    {
        float accel     = UnityInputModule.instance.controls.Player.Accelerate.ReadValue<float>();
        float deccel    = UnityInputModule.instance.controls.Player.Break.ReadValue<float>();
        float roll      = UnityInputModule.instance.controls.Player.TankStrafe.ReadValue<float>();
        float pitch     = UnityInputModule.instance.controls.Player.TankAccelerate.ReadValue<float>();

        Move(roll, pitch, accel, deccel);

        var hoverMotor = (vehicleParent.engine as HoverTankMotor);
        var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
        rigidbody.AddTorque(
            new Vector3(rot.x, rot.y, rot.z)
            * (1.0f - Utilities.ActualSmoothstep(hoverMotor.actualSpeed, hoverMotor.actualSpeed + 5.0f, vehicleParent.localVelocity.magnitude))
            * hoverFlipOverForce);
    }

    /// <summary>
    /// Important: Call this from a fixed update function.
    /// 
    /// Given inputs, this does the calculations to apply forces to the rigidbody for realistic flight. It considers drag,
    /// adjusting velocity based on speed/nose-direction, lift, and torques.
    /// </summary>
    private void Move(float roll, float pitch, float accel, float deccel)
    {
        float brakePower = brakeDrag * deccel;
        float brakeAccel = brakeDrag * brakePower;
        
        // Target an idle speed
        if (accel > 0.05f)
        {
            // If input thrust is over half pressed, OR the idle speed is lower than the input thrust, move to the input thrust
            if (accel > idleSpeedInput || accel > targetThrottleValue)
            {
                targetThrottleValue = Mathf.MoveTowards(targetThrottleValue, accel, Time.deltaTime);
            }
            // Otherwise, move to the idle input of 0.5
            else
            {
                targetThrottleValue = Mathf.MoveTowards(targetThrottleValue, 0.5f, Time.deltaTime * 0.1f);
            }
        }
        else if (deccel > 0.05f)
        {
            targetThrottleValue = Mathf.MoveTowards(targetThrottleValue, 0.0f, Time.deltaTime * 0.4f);
        }
        else if (targetThrottleValue > idleSpeedInput)
        {
            targetThrottleValue = Mathf.MoveTowards(targetThrottleValue, idleSpeedInput, Time.deltaTime * 0.02f);
        }

        throttleValue = Mathf.MoveTowards(throttleValue, targetThrottleValue, ((acceleration + brakeAccel)) * Time.deltaTime * throttleSpeedUpTime);

        Debug.Log("Target throttle value : " + targetThrottleValue);

        var localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        var forwardSpeed = Mathf.Max(0, localVelocity.z);

        CalculateDrag(brakeAccel, localVelocity.magnitude);
        CaluclateAerodynamicEffect(localVelocity.magnitude);

        // Main input to the rigidbody
        rigidbody.AddRelativeForce((
            (Vector3.forward * maxThrust * throttleValue * FORCE_MULT)                              // Driven Forces
            + (Vector3.forward * aeroFactor))      // Passive Forces
            / rigidbody.mass, ForceMode.Acceleration);
        
        Vector3 scaledTorque = new Vector3(Mathf.Min(pitch, Mathf.Lerp(0.2f, 1.0f, Utilities.ActualSmoothstep(15.0f, 20.0f, forwardSpeed))), 0.0f, -roll); 
        scaledTorque.Scale(turnTorques);
        
        rigidbody.AddRelativeTorque(scaledTorque * aeroFactor * FORCE_MULT, ForceMode.Force);

        // Bank effect: Separate from input control torque.
        float bankFactor = -transform.right.y;
        rigidbody.AddRelativeTorque(Vector3.up * bankFactor * aeroFactor * bankTorque * FORCE_MULT, ForceMode.Force);

        // Clamp the velocity
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, Mathf.Max(50.0f, topSpeed * targetThrottleValue));
    }

    /// <summary>
    /// increase the drag based on speed, since a constant drag doesn't seem "Real" (tm) enough
    /// Air brakes work by directly modifying drag. This part is actually pretty realistic!
    /// </summary>
    private void CalculateDrag(float airBrakes, float forwardSpeed)
    {
        float extraDrag = rigidbody.velocity.magnitude * dragIncreaseFactor;
        rigidbody.drag = Mathf.Lerp(originalDrag + extraDrag, (originalDrag + extraDrag) * airBrakesEffect, airBrakes);
    }

    /// <summary>
    /// "Aerodynamic" calculations. This is a very simple approximation of the effect that a plane
    /// will naturally try to align itself in the direction that it's facing when moving at speed.
    /// Without this, the plane would behave a bit like the asteroids spaceship!
    /// 
    /// also rotate the plane towards the direction of movement
    /// </summary>
    private void CaluclateAerodynamicEffect(float forwardSpeed)
    {
        if (rigidbody.velocity.magnitude > 0)
        {
            aeroFactor = Vector3.Dot(transform.forward, rigidbody.velocity.normalized);
            aeroFactor *= aeroFactor;

            var newVelocity = Vector3.Lerp(rigidbody.velocity, transform.forward * forwardSpeed, aeroFactor * forwardSpeed * aerodynamicEffect * Time.deltaTime);
            rigidbody.velocity = newVelocity;

            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, Quaternion.LookRotation(rigidbody.velocity, transform.up), aerodynamicEffect * Time.deltaTime);
        }

        // For lower speeds, reduce the drag to the original drag-coefficient so the plane falls realistically
        aeroFactor = Mathf.Max(0.2f, Mathf.Lerp(1.0f, aeroFactor, Utilities.ActualSmoothstep(5.0f, 20.0f, rigidbody.velocity.magnitude)));
    }
}
