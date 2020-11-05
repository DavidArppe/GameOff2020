using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVP;

public class JetMotorAndController : MonoBehaviour
{
    public VehicleParent vehicleParent;
    private new Rigidbody rigidbody;

    [Tooltip("How powerfully the plane can maneuver in each axis.\n\nX: Pitch\nY: Yaw\nZ: Roll")]
    public Vector3 turnTorques = new Vector3(60.0f, 10.0f, 90.0f);
    [Tooltip("Torque used by the magic banking force that rotates the plane when the plane is banked.")]
    public float bankTorque = 5.0f;
    [Tooltip("Power of the engine at max throttle.")]
    public float maxThrust = 3000.0f;
    [Tooltip("How quickly the jet can accelerate and decelerate.")]
    public float acceleration = 10.0f;
    [Tooltip("How quickly the jet will brake when the throttle goes below neutral.")]
    public float brakeDrag = 5.0f;

    public float jetDrag = 0.1f;
    public float jetAngularDrag = 5.0f;

    public float topSpeed = 200.0f;

    // Heavy things often require big numbers. It's nice to keep this multiplier on the
    // same scale as your mass to keep numbers small and manageable. For example, if your
    // game has mass in the hundreds, then use 100. If thousands, then 1000, etc.
    private const float FORCE_MULT = 100.0f;
    private float throttleValue = 0.0f;

    public float hoverFlipOverForce = 50.0f;

    private void Awake()
    {
        rigidbody = vehicleParent.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Making the center of mass the object's pivot makes its flight behavior much more
        // predictable and less reliant on the layout of its colliders.
        rigidbody.centerOfMass = Vector3.zero;
        m_OriginalAngularDrag = rigidbody.angularDrag;
        m_OriginalDrag = rigidbody.drag;
    }

    private void FixedUpdate()
    {
        float accel = UnityInputModule.instance.controls.Player.Accelerate.ReadValue<float>();
        float deccel = UnityInputModule.instance.controls.Player.Break.ReadValue<float>();
        float roll = UnityInputModule.instance.controls.Player.TankStrafe.ReadValue<float>();
        float pitch = UnityInputModule.instance.controls.Player.TankAccelerate.ReadValue<float>();

        Move(roll, pitch, accel, deccel);

        Vector2 globalVelocity2D = new Vector2(rigidbody.velocity.x, rigidbody.velocity.z);
        float stabilizeForce = Utilities.ActualSmoothstep(10.0f, 20.0f, globalVelocity2D.magnitude);

        var hoverMotor = (vehicleParent.engine as HoverTankMotor);
        var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
        rigidbody.AddTorque(
            new Vector3(rot.x, rot.y, rot.z)
            * (1.0f - Utilities.ActualSmoothstep(hoverMotor.actualSpeed, hoverMotor.actualSpeed + 5.0f, vehicleParent.localVelocity.magnitude))
            * hoverFlipOverForce);
    }

    private void Move(float roll, float pitch, float accel, float deccel)
    {
        // When the throttle goes below neutral, apply increased acceleration to slow down faster.
        float throttleTarget = accel;
        float brakePower = brakeDrag * deccel;
        float brakeAccel = brakeDrag * brakePower;

        // Throttle has to move slowly so that the plane still accelerates slowly using high
        // drag physics. Without them, the plane would change speed almost instantly.
        throttleValue = Mathf.MoveTowards(throttleValue, throttleTarget, ((acceleration + brakeAccel) / FORCE_MULT) * Time.deltaTime * throttleSpeedUpTime);


        var localVelocity = transform.InverseTransformDirection(rigidbody.velocity);
        var forwardSpeed = Mathf.Max(0, localVelocity.z);

        CalculateDrag(brakeAccel, forwardSpeed);
        CaluclateAerodynamicEffect(forwardSpeed);

        // Apply forces to the plane.
        rigidbody.AddRelativeForce((Vector3.forward * maxThrust * aeroFactor * throttleValue * FORCE_MULT) / rigidbody.mass, ForceMode.Acceleration);
        
        Vector3 scaledTorque = new Vector3(pitch, 0.0f, -roll); scaledTorque.Scale(turnTorques);
        rigidbody.AddRelativeTorque(scaledTorque * aeroFactor * FORCE_MULT, ForceMode.Force);

        // Apply magic forces when the plane is banked because it feels good. The principle
        // is that the plane rotates in the direction you're banked. The more banked you are
        // (up to a max of 90 degrees) the more it magically turns in that direction.
        float bankFactor = -transform.right.y;
        rigidbody.AddRelativeTorque(Vector3.up * bankFactor * aeroFactor * bankTorque * FORCE_MULT, ForceMode.Force);

        // We can later scale this based on the height in the atmosphere
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, topSpeed);
    }

    [SerializeField] private float m_AirBrakesEffect = 3f;        // How much the air brakes effect the drag.
    [SerializeField] private float m_DragIncreaseFactor = 0.001f; // how much drag should increase with speed.
    private float m_OriginalDrag;         // The drag when the scene starts.
    private float m_OriginalAngularDrag;  // The angular drag when the scene starts.
    private void CalculateDrag(float airBrakes, float forwardSpeed)
    {
        // increase the drag based on speed, since a constant drag doesn't seem "Real" (tm) enough
        float extraDrag = rigidbody.velocity.magnitude * m_DragIncreaseFactor;
        // Air brakes work by directly modifying drag. This part is actually pretty realistic!
        rigidbody.drag = Mathf.Lerp(m_OriginalDrag + extraDrag, (m_OriginalDrag + extraDrag) * m_AirBrakesEffect, airBrakes);
    }

    [SerializeField] private float m_AerodynamicEffect = 0.02f;   // How much aerodynamics affect the speed of the aeroplane.
    private float aeroFactor;
    private float throttleSpeedUpTime = 4.0f;

    private void CaluclateAerodynamicEffect(float forwardSpeed)
    {
        // "Aerodynamic" calculations. This is a very simple approximation of the effect that a plane
        // will naturally try to align itself in the direction that it's facing when moving at speed.
        // Without this, the plane would behave a bit like the asteroids spaceship!
        if (rigidbody.velocity.magnitude > 0)
        {
            // compare the direction we're pointing with the direction we're moving:
            aeroFactor = Vector3.Dot(transform.forward, rigidbody.velocity.normalized);
            // multipled by itself results in a desirable rolloff curve of the effect
            aeroFactor *= aeroFactor;
            // Finally we calculate a new velocity by bending the current velocity direction towards
            // the the direction the plane is facing, by an amount based on this aeroFactor
            var newVelocity = Vector3.Lerp(rigidbody.velocity, transform.forward * forwardSpeed,
                                           aeroFactor * forwardSpeed * m_AerodynamicEffect * Time.deltaTime);
            rigidbody.velocity = newVelocity;

            // also rotate the plane towards the direction of movement - this should be a very small effect, but means the plane ends up
            // pointing downwards in a stall
            rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation,
                                                  Quaternion.LookRotation(rigidbody.velocity, transform.up),
                                                  m_AerodynamicEffect * Time.deltaTime);
        }

        aeroFactor = Mathf.Max(0.2f, Mathf.Lerp(1.0f, aeroFactor, Utilities.ActualSmoothstep(5.0f, 20.0f, rigidbody.velocity.magnitude)));
    }
}
