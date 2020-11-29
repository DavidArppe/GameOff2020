using UnityEngine;
using System.Collections.Generic;

namespace UnityMovementAI
{
    public class HideUnit : MonoBehaviour
    {
        public Transform hideableObstacleParent;
        public Transform target;

        List<MovementAIRigidbody> objectsToHideBehind;
        SteeringBasics steeringBasics;
        Hide hide;

        WallAvoidance wallAvoid;

        void Start()
        {
            steeringBasics = transform.parent.GetComponent<SteeringBasics>();
            hide = GetComponent<Hide>();

            wallAvoid = GetComponent<WallAvoidance>();

            objectsToHideBehind = new List<MovementAIRigidbody>(hideableObstacleParent.GetComponentsInChildren<MovementAIRigidbody>());
        }

        void FixedUpdate()
        {
            Vector3 hidePosition;
            Vector3 hideAccel = hide.GetSteering(target, objectsToHideBehind, out hidePosition);

            Vector3 accel = wallAvoid.GetSteering(hidePosition - transform.position);

            if (accel.magnitude < 0.005f)
            {
                accel = hideAccel;
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}