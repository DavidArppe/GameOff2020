using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class FollowCam : PivotBasedCameraRig
{
    [System.Serializable]
    public class FollowCamSettings
    {
        public Transform target; // The target object to follow
        public Transform pivot;  // The object to pivot around

        public float cameraVerticalFOV  = 60.0f;
        public float spinTurnLimit      = 90;   // The threshold beyond which the camera stops following the target's rotation. (used in situations where a car spins out, for example)
        public bool  followTilt         = true; // Whether the rig will tilt (around X axis) with the target.
        public float moveSpeed          = 3;    // How fast the rig will move to keep up with target's position
        public float turnSpeed          = 1;    // How fast the rig will turn to keep up with target's rotation
        public float rollSpeed          = 0.2f; // How fast the rig will roll (around Z axis) to match target's roll.

        [HideInInspector]
        public Vector3 originalTargetOffsetFromPivot;
    }
    
    public float rotationSpeed = 100.0f;

    public FollowCamSettings carSettings;
    public FollowCamSettings hoverTankSettings;
    public FollowCamSettings jetSettings;

    public VehicleTypeSwitch switcher;

    private float lastFlatAngle; // The relative angle of the target and the rig from the previous frame.
    private float currentTurnAmount; // How much to turn the camera
    private float turnSpeedVelocityChange; // The change in the turn speed velocity
    private Vector3 rollUp = Vector3.up;// The roll of the camera around the z axis ( generally this will always just be up )

    private Vector3 interpolatedTargetPosition;
    private Vector3 interpolatedTargetForward;
    private Vector3 interpolatedTargetUp;
    private Vector3 interpolatedPivotPosition;
    private Vector3 interpolatedPivotForward;
    private Vector3 interpolatedPivotUp;

    private FollowCamSettings interpVals;
    private Rigidbody rigid;
    private Camera targetCamera;

    private Vector2 rotationAboutPivot = Vector2.zero;
    private float currentVelX, currentVelY;

    private void Start()
    {
        interpVals = new FollowCamSettings();
        targetCamera = GetComponent<Camera>();

        rigid = switcher.GetComponent<Rigidbody>();

        hoverTankSettings.originalTargetOffsetFromPivot = hoverTankSettings.target.position - hoverTankSettings.pivot.position;
        carSettings.originalTargetOffsetFromPivot       = carSettings.target.position - carSettings.pivot.position;
        jetSettings.originalTargetOffsetFromPivot       = jetSettings.target.position - jetSettings.pivot.position;
    }

    // Basic 3 way interpolation. Not quite bilinear, but since we likely won't have 3-way transitions, this is fine.
    void CalculateInterpolatedCameraValues(FollowCamSettings c, FollowCamSettings h, FollowCamSettings j)
    {
        interpolatedPivotPosition = Vector3.Lerp(Vector3.Lerp(c.pivot.position, h.pivot.position, switcher.isHoverLerpValue), j.pivot.position, switcher.isJetLerpValue);
        interpolatedPivotForward = Vector3.Lerp(Vector3.Lerp(c.pivot.forward, h.pivot.forward, switcher.isHoverLerpValue), j.pivot.forward, switcher.isJetLerpValue);
        interpolatedPivotUp = Vector3.Lerp(Vector3.Lerp(c.pivot.up, h.pivot.up, switcher.isHoverLerpValue), j.pivot.up, switcher.isJetLerpValue);

        interpolatedTargetPosition  = Vector3.Lerp(Vector3.Lerp(c.target.position, h.target.position, switcher.isHoverLerpValue), j.target.position, switcher.isJetLerpValue);
        interpolatedTargetForward   = Vector3.Lerp(Vector3.Lerp(c.target.forward, h.target.forward, switcher.isHoverLerpValue), j.target.forward, switcher.isJetLerpValue);
        interpolatedTargetUp        = Vector3.Lerp(Vector3.Lerp(c.target.up, h.target.up, switcher.isHoverLerpValue), j.target.up, switcher.isJetLerpValue);

        interpVals.spinTurnLimit    = Mathf.Lerp(Mathf.Lerp(c.spinTurnLimit, h.spinTurnLimit, switcher.isHoverLerpValue), j.spinTurnLimit, switcher.isJetLerpValue);
        interpVals.followTilt       = switcher.isJetLerpValue > 0.5f ? j.followTilt : switcher.isHoverLerpValue > 0.5f ? h.followTilt : c.followTilt;
        interpVals.moveSpeed        = Mathf.Lerp(Mathf.Lerp(c.moveSpeed, h.moveSpeed, switcher.isHoverLerpValue), j.moveSpeed, switcher.isJetLerpValue);
        interpVals.turnSpeed        = Mathf.Lerp(Mathf.Lerp(c.turnSpeed, h.turnSpeed, switcher.isHoverLerpValue), j.turnSpeed, switcher.isJetLerpValue);
        interpVals.rollSpeed        = Mathf.Lerp(Mathf.Lerp(c.rollSpeed, h.rollSpeed, switcher.isHoverLerpValue), j.rollSpeed, switcher.isJetLerpValue);
    }

    public static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    void RotateCamera(Vector2 rotateAmount)
    {
        rotationAboutPivot += rotateAmount * Time.deltaTime * rotationSpeed;

        var dirToPoint = interpolatedTargetPosition - interpolatedPivotPosition;

        Matrix4x4 mat = new Matrix4x4(
            -Vector3.Cross(interpolatedPivotForward, interpolatedPivotUp),
            interpolatedPivotUp,
            interpolatedPivotForward,
            new Vector4(interpolatedPivotPosition.x, interpolatedPivotPosition.y, interpolatedPivotPosition.z, 1.0f)
            );

        dirToPoint = mat.inverse.MultiplyVector(dirToPoint);
        dirToPoint = Quaternion.Euler(new Vector3(rotationAboutPivot.y, rotationAboutPivot.x, 0.0f)) * dirToPoint;
        dirToPoint = mat.MultiplyVector(dirToPoint);

        interpolatedTargetPosition = interpolatedPivotPosition + dirToPoint;

        interpolatedTargetForward = Vector3.Normalize(-dirToPoint);
        interpolatedTargetUp = -Vector3.Cross(interpolatedTargetForward, Vector3.Cross(interpolatedTargetForward, interpolatedPivotUp));
    }

    protected override void FollowTarget(float deltaTime)
    {
        CalculateInterpolatedCameraValues(carSettings, hoverTankSettings, jetSettings);

        float speedCalculatedRotationSpeed = Mathf.Clamp01(Mathf.InverseLerp(1.0f, 50.0f, rigid.velocity.magnitude)) * 250.0f;

        Vector2 rotateAmount = UnityInputModule.instance.controls.Player.Camera.ReadValue<Vector2>();
        RotateCamera(rotateAmount);

        float inputScaler = 1.0f - Utilities.ActualSmoothstep(0.05f, 0.15f, Mathf.Max(Mathf.Abs(rotateAmount.x), Mathf.Abs(rotateAmount.y)));
        rotationAboutPivot = new Vector2(
            Mathf.SmoothDampAngle(rotationAboutPivot.x, 0.0f, ref currentVelX, 0.2f, speedCalculatedRotationSpeed * inputScaler),
            Mathf.SmoothDampAngle(rotationAboutPivot.y, 0.0f, ref currentVelY, 0.2f, speedCalculatedRotationSpeed * inputScaler));

        // if no target, or no time passed then we quit early, as there is nothing to do
        if (!(deltaTime > 0) || target == null)
        {
            return;
        }

        // initialise some vars, we'll be modifying these in a moment
        var targetForward = interpolatedTargetForward;
        var targetUp = interpolatedTargetUp;

        // This section allows the camera to stop following the target's rotation when the target is spinning too fast.
        // eg when a car has been knocked into a spin. The camera will resume following the rotation
        // of the target when the target's angular velocity slows below the threshold.
        var currentFlatAngle = Mathf.Atan2(targetForward.x, targetForward.z)*Mathf.Rad2Deg;
        if (interpVals.spinTurnLimit > 0)
        {
            var targetSpinSpeed = Mathf.Abs(Mathf.DeltaAngle(lastFlatAngle, currentFlatAngle))/deltaTime;
            var desiredTurnAmount = Mathf.InverseLerp(interpVals.spinTurnLimit, interpVals.spinTurnLimit *0.75f, targetSpinSpeed);
            var turnReactSpeed = (currentTurnAmount > desiredTurnAmount ? .1f : 1f);
            if (Application.isPlaying)
            {
                currentTurnAmount = Mathf.SmoothDamp(currentTurnAmount, desiredTurnAmount,
                                                        ref turnSpeedVelocityChange, turnReactSpeed);
            }
            else
            {
                // for editor mode, smoothdamp won't work because it uses deltaTime internally
                currentTurnAmount = desiredTurnAmount;
            }
        }
        else
        {
            currentTurnAmount = 1;
        }
        lastFlatAngle = currentFlatAngle;

        // camera position moves towards target position:
        transform.position = Vector3.Lerp(transform.position, interpolatedTargetPosition, deltaTime* interpVals.moveSpeed);

        // camera's rotation is split into two parts, which can have independend speed settings:
        // rotating towards the target's forward direction (which encompasses its 'yaw' and 'pitch')
        if (!interpVals.followTilt)
        {
            targetForward.y = 0;
            if (targetForward.sqrMagnitude < float.Epsilon)
            {
                targetForward = transform.forward;
            }
        }
        var rollRotation = Quaternion.LookRotation(targetForward, rollUp);

        // and aligning with the target object's up direction (i.e. its 'roll')
        rollUp = interpVals.rollSpeed > 0 ? Vector3.Slerp(rollUp, targetUp, interpVals.rollSpeed *deltaTime) : Vector3.up;
        transform.rotation = Quaternion.Lerp(transform.rotation, rollRotation, interpVals.turnSpeed *currentTurnAmount*deltaTime);
    }
}
