using UnityEngine;
using System.Collections;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Hover/Hover Tank Motor", 0)]

    //Motor subclass for hovering vehicles
    public class HoverTankMotor : Motor
    {
        [Header("Performance")]

        [Tooltip("Curve which calculates the driving force based on the speed of the vehicle, x-axis = speed, y-axis = force")]
        public AnimationCurve forceCurve = AnimationCurve.EaseInOut(0, 1, 50, 0);
        public HoverTankWheel[] wheels;
        public HoverTankSteer steer;

        public float actualSpeed = 15f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Get proper input
            float actualAccel = UnityInputModule.instance.controls.Player.TankAccelerate.ReadValue<float>();

            if (steer.disableMoveBackwards)
            {
                actualInput = inputCurve.Evaluate(Mathf.Abs(actualAccel)) * Mathf.Clamp01(actualAccel);
            }
            else
            {
                actualInput = inputCurve.Evaluate(Mathf.Abs(actualAccel)) * Mathf.Sign(actualAccel);
            }

            //Set hover wheel speeds and forces
            foreach (HoverTankWheel curWheel in wheels)
            {
                if (ignition)
                {
                    float boostEval = boostPowerCurve.Evaluate(Mathf.Abs(vp.localVelocity.z));
                    curWheel.targetSpeed = actualInput * actualSpeed * 0.1f;
                    curWheel.targetForce = Mathf.Abs(actualInput) * health;
                }
                else
                {
                    curWheel.targetSpeed = 0;
                    curWheel.targetForce = 0;
                }

                curWheel.doFloat = ignition && health > 0;
            }
        }

        public override void Update()
        {
            // //Set engine pitch
            // if (snd && ignition)
            // {
            //     targetPitch = Mathf.Max(Mathf.Abs(actualInput), Mathf.Abs(vp.steerInput)) * 
            //         Mathf.Max(1 - forceCurve.Evaluate(Mathf.Abs(vp.localVelocity.z)), 1 - forceCurve.Evaluate(Mathf.Abs(vp.localVelocity.x)));
            // }

            base.Update();
        }
    }
}