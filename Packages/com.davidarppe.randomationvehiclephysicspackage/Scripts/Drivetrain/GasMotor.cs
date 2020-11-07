using UnityEngine;
using System.Collections;

namespace RVP
{
    [RequireComponent(typeof(DriveForce))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Drivetrain/Gas Motor", 0)]

    //Motor subclass for internal combustion engines
    public class GasMotor : Motor
    {
        [Header("Performance")]

        [Tooltip("X-axis = RPM in thousands, y-axis = torque.  The rightmost key represents the maximum RPM")]
        public AnimationCurve torqueCurve = AnimationCurve.EaseInOut(0, 0, 8, 1);

        [Range(0, 0.99f)]
        [Tooltip("How quickly the engine adjusts its RPMs")]
        public float inertia;

        [Tooltip("Can the engine turn backwards?")]
        public bool canReverse;
        DriveForce targetDrive;
        [System.NonSerialized]
        public float maxRPM;

        public DriveForce[] outputDrives;

        [Tooltip("Exponent for torque output on each wheel")]
        public float driveDividePower = 3;
        float actualAccel;

        [Header("FMOD Audio")]
        public FMODUnity.StudioEventEmitter bodyNoiseSoundEmitter;
        public FMODUnity.StudioEventEmitter engineSoundEmitter;
        public FMODUnity.StudioEventEmitter wheelsSoundEmitter;
        [HideInInspector] public float bodyNoiseVolume = 1.0f;
        [HideInInspector] public float engineVolume = 1.0f;
        [HideInInspector] public float wheelsVolume = 1.0f;

        public bool rpmIncreaseBetweenShifts;
        public float rpmInertia = 2.0f;

        [System.NonSerialized]
        public float targetPitch;
        protected float pitchFactor;
        protected float airPitch;

        [Header("Transmission")]
        public GearboxTransmission transmission;
        [System.NonSerialized]
        public bool shifting;

        private float currentCompressionVelocity    = 0.0f;
        private float smoothTargetCompression       = 0.0f;
        private float compression                   = 0.0f;
        private float smoothTargetPitch             = 0.0f;
        private float pitchVelocity                 = 0.0f;
        private float smoothedThrottleVelocity      = 0.0f;
        private float smoothedThrottleInput         = 0.0f;

        public override void Start()
        {
            base.Start();
            targetDrive = GetComponent<DriveForce>();
            //Get maximum possible RPM
            GetMaxRPM();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //Calculate proper input
            actualAccel = Mathf.Lerp(vp.brakeIsReverse && vp.reversing && vp.accelInput <= 0 ? vp.brakeInput : vp.accelInput, Mathf.Max(vp.accelInput, vp.burnout), vp.burnout);
            float accelGet = canReverse ? actualAccel : Mathf.Clamp01(actualAccel);
            actualInput = inputCurve.Evaluate(Mathf.Abs(accelGet)) * Mathf.Sign(accelGet);
            targetDrive.curve = torqueCurve;

            if (ignition)
            {
                float boostEval = boostPowerCurve.Evaluate(Mathf.Abs(vp.localVelocity.z));
                //Set RPM
                targetDrive.rpm = Mathf.Lerp(targetDrive.rpm, actualInput * maxRPM * 1000 * (boosting ? 1 + boostEval : 1), (1 - inertia) * Time.timeScale);
                //Set torque
                if (targetDrive.feedbackRPM > targetDrive.rpm)
                {
                    targetDrive.torque = 0;
                }
                else
                {
                    targetDrive.torque = torqueCurve.Evaluate(targetDrive.feedbackRPM * 0.001f - (boosting ? boostEval : 0)) * Mathf.Lerp(targetDrive.torque, power * Mathf.Abs(System.Math.Sign(actualInput)), (1 - inertia) * Time.timeScale) * (boosting ? 1 + boostEval : 1) * health;
                }

                //Send RPM and torque through drivetrain
                if (outputDrives.Length > 0)
                {
                    float torqueFactor = Mathf.Pow(1f / outputDrives.Length, driveDividePower);
                    float tempRPM = 0;

                    foreach (DriveForce curOutput in outputDrives)
                    {
                        tempRPM += curOutput.feedbackRPM;
                        curOutput.SetDrive(targetDrive, torqueFactor);
                    }

                    targetDrive.feedbackRPM = tempRPM / outputDrives.Length;
                }

                if (transmission)
                {
                    shifting = transmission.shiftTime > 0;
                }
                else
                {
                    shifting = false;
                }
            }
            else
            {
                //If turned off, set RPM and torque to 0 and distribute it through drivetrain
                targetDrive.rpm = 0;
                targetDrive.torque = 0;
                targetDrive.feedbackRPM = 0;
                shifting = false;

                if (outputDrives.Length > 0)
                {
                    foreach (DriveForce curOutput in outputDrives)
                    {
                        curOutput.SetDrive(targetDrive);
                    }
                }
            }
        }

        public void GetMaxRPM()
        {
            maxRPM = torqueCurve.keys[torqueCurve.length - 1].time;

            if (outputDrives.Length > 0)
            {
                foreach (DriveForce curOutput in outputDrives)
                {
                    curOutput.curve = targetDrive.curve;

                    if (curOutput.GetComponent<Transmission>())
                    {
                        curOutput.GetComponent<Transmission>().ResetMaxRPM();
                    }
                }
            }
        }

        public override void Update()
        {
            //Set audio pitch
            if (ignition)
            {
                airPitch = vp.groundedWheels > 0 && actualAccel != 0 ? 1 : (vp.groundedWheels < 1 && actualAccel != 0) ? 1.05f : Mathf.Lerp(airPitch, 0, 0.5f * Time.deltaTime);
                pitchFactor = (actualAccel != 0 || vp.groundedWheels == 0 ? 1 : 0.75f) * (shifting ? (rpmIncreaseBetweenShifts ? Mathf.Sin((transmission.shiftTime / transmission.shiftDelay) * Mathf.PI) : Mathf.Min(transmission.shiftDelay, Mathf.Pow(transmission.shiftTime, 2)) / transmission.shiftDelay) : 1) * airPitch;
                targetPitch = Mathf.Abs((targetDrive.feedbackRPM * 0.001f) / maxRPM) * pitchFactor;
                smoothTargetPitch = Mathf.SmoothDamp(smoothTargetPitch, targetPitch, ref pitchVelocity, rpmInertia);

                engineSoundEmitter.SetParameter("rpms", Mathf.LerpUnclamped(1350.0f, 6200.0f, Mathf.Clamp(smoothTargetPitch, 0.0f, 1.05f)));
            }
            else
            {
                engineSoundEmitter.SetParameter("rpms", targetDrive.rpm);
            }

            float speed = vp.rb.velocity.magnitude;

            compression = compression * 0.999f;
            foreach (var wheel in vp.wheels)
            {
                if (wheel.suspensionParent)
                {
                    compression = Mathf.Max(compression, 1.0f - wheel.suspensionParent.compression);
                }
            }

            float impactCompression = Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(0.775f, 1.0f, compression));
            float speedCompression = Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(5.0f, 60.0f, speed));
            compression = Mathf.Max(impactCompression * 0.5f, speedCompression * 0.2f);
            smoothTargetCompression = Mathf.SmoothDamp(smoothTargetCompression, compression, ref currentCompressionVelocity, 0.05f);

            smoothedThrottleInput = Mathf.SmoothDamp(smoothedThrottleInput, actualInput, ref smoothedThrottleVelocity, rpmInertia);

            engineSoundEmitter.SetParameter("throttle", smoothedThrottleInput);
            wheelsSoundEmitter.SetParameter("speed", vp.groundedWheels > 0 ? speed : 0.0f);
            bodyNoiseSoundEmitter.SetParameter("susp_travel_speed", smoothTargetCompression * 0.75f);
            bodyNoiseSoundEmitter.EventInstance.setVolume(bodyNoiseVolume * (Mathf.SmoothStep(0.0f, 1.0f, Mathf.InverseLerp(15.0f, 60.0f, speed)) * 0.4f + 0.6f));

            base.Update();
        }
    }
}