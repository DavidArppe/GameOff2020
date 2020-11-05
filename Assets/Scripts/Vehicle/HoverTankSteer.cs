using UnityEngine;
using System.Collections;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Hover/Hover Tank Steer", 2)]

    //Class for steering hover vehicles
    public class HoverTankSteer : MonoBehaviour
    {
        Transform tr;
        VehicleParent vp;
        public float flipOverForce = 50.0f;
        public float steerRate = 1;
        float steerAmount;

        [Tooltip("Curve for limiting steer range based on speed, x-axis = speed, y-axis = multiplier")]
        public AnimationCurve steerCurve = AnimationCurve.Linear(0, 1, 30, 0.1f);

        [Tooltip("Horizontal stretch of the steer curve")]
        public float steerCurveStretch = 1;
        public HoverTankWheel[] steeredFrontWheels;
        public HoverTankWheel[] steeredRearWheels;
        public float steerSpeed = 0.5f;

        [Header("Visual")]

        public bool rotate;
        public float maxDegreesRotation;
        public float rotationOffset;
        float steerRot;

        private Rigidbody parentRigidbody;

        private bool stabilizeOrientation = true;

        public void ToggleJetHover(bool isJet)
        {
            stabilizeOrientation = !isJet;

            foreach (HoverTankWheel wheel in steeredFrontWheels)
            {
                wheel.applyFloatDriveUnscaled = !isJet;
            }
            foreach (HoverTankWheel wheel in steeredRearWheels)
            {
                wheel.applyFloatDriveUnscaled = !isJet;
            }
        }

        void Start()
        {
            tr = transform;
            vp = tr.GetTopmostParentComponent<VehicleParent>();
            parentRigidbody = vp.GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            //Set steering of hover wheels
            float rbSpeed = vp.localVelocity.z / steerCurveStretch;
            float steerLimit = steerCurve.Evaluate(Mathf.Abs(rbSpeed));
            steerAmount = vp.steerInput * steerLimit;

            var hoverMotor = vp.engine as HoverTankMotor;

            if (stabilizeOrientation)
            {
                var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
                parentRigidbody.AddTorque(new Vector3(rot.x, rot.y, rot.z) * flipOverForce);
            }

            float steerDirection = UnityInputModule.instance.controls.Player.TankTurn.ReadValue<float>() * steerSpeed;

            foreach (HoverTankWheel curWheel in steeredFrontWheels)
            {
                curWheel.steerRate = (steerAmount + steerDirection) * steerRate;
            }

            foreach (HoverTankWheel curWheel in steeredRearWheels)
            {
                curWheel.steerRate = (steerAmount - steerDirection) * steerRate;
            }
        }

        void Update()
        {
            if (rotate)
            {
                steerRot = Mathf.Lerp(steerRot, steerAmount * maxDegreesRotation + rotationOffset, steerRate * 0.1f * Time.timeScale);
                tr.localEulerAngles = new Vector3(tr.localEulerAngles.x, tr.localEulerAngles.y, steerRot);
            }
        }
    }
}
