using UnityEngine;
using System.Collections;

namespace RVP
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Hover/Hover Tank Wheel", 1)]

    //Class for hover vehicle wheels
    public class HoverTankWheel : HoverWheel
    {
        Transform trHover;
        Rigidbody rbHover;
        VehicleParent vpHover;

        Vector3 upDirHover;//Local up direction

        float compressionHover;//How compressed the suspension is
        float flippedSideFactorHover;//Multiplier for inverting the forces on opposite sides

        GameObject detachedWheelHover;
        MeshCollider detachedColHover;
        Rigidbody detachedBodyHover;
        MeshFilter detachFilterHover;

        Rigidbody parentRigidbody;
        public bool applyFloatDriveUnscaled;

        void Start()
        {
            trHover = transform;
            rbHover = trHover.GetTopmostParentComponent<Rigidbody>();
            vpHover = trHover.GetTopmostParentComponent<VehicleParent>();
            flippedSideFactorHover = Vector3.Dot(trHover.forward, vpHover.transform.right) < 0 ? 1 : -1;
            canDetach = detachForce < Mathf.Infinity && Application.isPlaying;
            bufferDistance = Mathf.Min(hoverDistance, bufferDistance);

            if (canDetach)
            {
                detachedWheelHover = new GameObject(vpHover.transform.name + "'s Detached Wheel");
                detachedWheelHover.layer = LayerMask.NameToLayer("Detachable Part");
                detachFilterHover = detachedWheelHover.AddComponent<MeshFilter>();
                detachFilterHover.sharedMesh = visualWheel.GetComponent<MeshFilter>().sharedMesh;
                MeshRenderer detachRend = detachedWheelHover.AddComponent<MeshRenderer>();
                detachRend.sharedMaterial = visualWheel.GetComponent<MeshRenderer>().sharedMaterial;
                detachedColHover = detachedWheelHover.AddComponent<MeshCollider>();
                detachedColHover.convex = true;
                detachedBodyHover = detachedWheelHover.AddComponent<Rigidbody>();
                detachedBodyHover.mass = mass;
                detachedWheelHover.SetActive(false);
            }

            parentRigidbody = vpHover.GetComponent<Rigidbody>();
        }

        void Update()
        {
            //Tilt the visual wheel
            if (visualWheel && connected)
            {
                TiltWheel();
            }
        }

        void FixedUpdate()
        {
            upDirHover = trHover.up;

            if (getContact)
            {
                GetWheelContact();
            }
            else if (grounded)
            {
                contactPoint.point += rbHover.GetPointVelocity(trHover.position) * Time.fixedDeltaTime;
            }

            compressionHover = Mathf.Clamp01(contactPoint.distance / (hoverDistance));

            if (grounded && doFloat && connected)
            {
                ApplyFloat();
                //if (applyFloatDrive)
                //{
                    ApplyFloatDrive();
                //}
            }
        }

        //Get the contact point of the wheel
        void GetWheelContact()
        {
            RaycastHit hit = new RaycastHit();
            Vector3 localVel = rbHover.GetPointVelocity(trHover.position);
            RaycastHit[] wheelHits = Physics.RaycastAll(trHover.position, -upDirHover, hoverDistance, GlobalControl.wheelCastMaskStatic);
            bool validHit = false;
            float hitDist = Mathf.Infinity;

            //Loop through contact points to get the closest one
            foreach (RaycastHit curHit in wheelHits)
            {
                if (!curHit.transform.IsChildOf(vpHover.tr) && curHit.distance < hitDist)
                {
                    hit = curHit;
                    hitDist = curHit.distance;
                    validHit = true;
                }
            }

            //Set contact point variables
            if (validHit)
            {
                if (!hit.collider.transform.IsChildOf(vpHover.tr))
                {
                    grounded = true;
                    contactPoint.distance = hit.distance;
                    contactPoint.point = hit.point + localVel * Time.fixedDeltaTime;
                    contactPoint.grounded = true;
                    contactPoint.normal = hit.normal;
                    contactPoint.relativeVelocity = trHover.InverseTransformDirection(localVel);
                    contactPoint.col = hit.collider;
                }
            }
            else
            {
                grounded = false;
                contactPoint.distance = hoverDistance;
                contactPoint.point = Vector3.zero;
                contactPoint.grounded = false;
                contactPoint.normal = upDirHover;
                contactPoint.relativeVelocity = Vector3.zero;
                contactPoint.col = null;
            }
        }

        //Make the vehicle hover
        void ApplyFloat()
        {
            if (grounded)
            {
                //Get the vertical speed of the wheel
                float travelVel = vpHover.norm.InverseTransformDirection(rbHover.GetPointVelocity(trHover.position)).z;

                rbHover.AddForceAtPosition(upDirHover * floatForce * (Mathf.Pow(floatForceCurve.Evaluate(1 - compressionHover), Mathf.Max(1, floatExponent)) - floatDampening * Mathf.Clamp(travelVel, -1, 1))
                    , trHover.position
                    , vpHover.suspensionForceMode);

                if (contactPoint.distance < bufferDistance)
                {
                    rbHover.AddForceAtPosition(-upDirHover * bufferFloatForce * floatForceCurve.Evaluate(contactPoint.distance / bufferDistance) * Mathf.Clamp(travelVel, -1, 0)
                        , trHover.position
                        , vpHover.suspensionForceMode);
                }
            }
        }

        //Drive the vehicle
        void ApplyFloatDrive()
        {
            float jetSpeedModifier = 1.0f;
            if (!applyFloatDriveUnscaled)
            {
                var hoverMotor = vpHover.engine as HoverTankMotor;
                var relativeVelocity = vpHover.transform.InverseTransformDirection(vpHover.rb.velocity);
                jetSpeedModifier = 1.0f - Utilities.ActualSmoothstep(VehicleTypeSwitch.jetAnimationStartVel, VehicleTypeSwitch.jetAnimationEndVel, relativeVelocity.z);
            }

            rbHover.AddForceAtPosition(
                trHover.TransformDirection(
                    Mathf.Clamp(targetSpeed, -1, 1) * targetForce * steerFactor * flippedSideFactorHover - contactPoint.relativeVelocity.x * sideFriction,
                    0,
                    -steerRate * steerFactor * flippedSideFactorHover - contactPoint.relativeVelocity.z * sideFriction) * (1 - compressionHover) * jetSpeedModifier,
                trHover.position,
                vpHover.wheelForceMode);
        }

        //Tilt the visual wheel
        void TiltWheel()
        {
            float sideTilt = Mathf.Clamp(-steerRate * steerFactor * flippedSideFactorHover - Mathf.Clamp(contactPoint.relativeVelocity.z * 0.1f, -1, 1) * sideFriction, -1, 1);
            float actualBrake = 0.0f;
            float forwardTilt = Mathf.Clamp((Mathf.Clamp(targetSpeed, -1, 1) * targetForce - actualBrake * Mathf.Clamp(contactPoint.relativeVelocity.x * 0.1f, -1, 1) * flippedSideFactorHover) * flippedSideFactorHover, -1, 1);

            visualWheel.localRotation = Quaternion.Lerp(visualWheel.localRotation, Quaternion.LookRotation(new Vector3(-forwardTilt * visualTiltAmount, -1 + Mathf.Abs(F.MaxAbs(sideTilt, forwardTilt)) * visualTiltAmount, -sideTilt * visualTiltAmount).normalized, Vector3.forward), visualTiltRate * Time.deltaTime);
        }

        void OnDrawGizmosSelected()
        {
            trHover = transform;
            //Draw a ray to show the distance of the "suspension"
            Gizmos.color = Color.white;
            Gizmos.DrawRay(trHover.position, -trHover.up * hoverDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(trHover.position, -trHover.up * bufferDistance);
        }

        //Destroy detached wheel
        void OnDestroy()
        {
            if (detachedWheelHover)
            {
                Destroy(detachedWheelHover);
            }
        }
    }
}