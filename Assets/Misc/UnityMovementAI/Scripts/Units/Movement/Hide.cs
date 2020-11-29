using UnityEngine;
using System.Collections.Generic;

namespace UnityMovementAI
{
    [RequireComponent(typeof(Evade))]
    public class Hide : MonoBehaviour
    {
        public float distanceFromBoundary = 0.6f;
        public float minDistanceTargetToHidingSpot = 8.0f;

        SteeringBasics steeringBasics;
        Evade evade;

        void Awake()
        {
            steeringBasics = transform.parent.GetComponent<SteeringBasics>();
            evade = GetComponent<Evade>();
        }

        public Vector3 GetSteering(MovementAIRigidbody target, ICollection<MovementAIRigidbody> obstacles)
        {
            Vector3 bestHidingSpot;
            return GetSteering(target, obstacles, out bestHidingSpot);
        }

        public Vector3 GetSteering(Transform target, ICollection<MovementAIRigidbody> obstacles)
        {
            Vector3 bestHidingSpot;
            return GetSteering(target, obstacles, out bestHidingSpot);
        }

        public Vector3 GetSteering(MovementAIRigidbody target, ICollection<MovementAIRigidbody> obstacles, out Vector3 bestHidingSpot)
        {
            /* Find the closest hiding spot. */
            float distToClostest = Mathf.Infinity;
            bestHidingSpot = Vector3.zero;

            foreach (MovementAIRigidbody r in obstacles)
            {
                Vector3 hidingSpot = GetHidingPosition(r, target);

                float dist = Vector3.Distance(hidingSpot, transform.position);

                if (dist < distToClostest)
                {
                    distToClostest = dist;
                    bestHidingSpot = hidingSpot;
                }
            }

            /* If no hiding spot is found then just evade the enemy. */
            if (distToClostest == Mathf.Infinity)
            {
                return evade.GetSteering(target);
            }

            //Debug.DrawLine(transform.position, bestHidingSpot);

            return steeringBasics.Arrive(bestHidingSpot);
        }

        public Vector3 GetSteering(Transform target, ICollection<MovementAIRigidbody> obstacles, out Vector3 bestHidingSpot)
        {
            /* Find the closest hiding spot. */
            float distToClostest = Mathf.Infinity;
            bestHidingSpot = Vector3.zero;

            foreach (MovementAIRigidbody r in obstacles)
            {
                Vector3 hidingSpot = GetHidingPosition(r, target);

                float dist = Vector3.Distance(hidingSpot, transform.position);

                if (dist < distToClostest && Vector3.Distance(hidingSpot, new Vector3(target.position.x, 0.0f, target.position.z)) > minDistanceTargetToHidingSpot)
                {
                    distToClostest = dist;
                    bestHidingSpot = hidingSpot;
                }
            }

            /* If no hiding spot is found then just evade the enemy. */
            if (distToClostest == Mathf.Infinity)
            {
                return evade.GetSteering(target);
            }

            //Debug.DrawLine(transform.position, bestHidingSpot);

            return steeringBasics.Arrive(bestHidingSpot);
        }

        Vector3 GetHidingPosition(MovementAIRigidbody obstacle, MovementAIRigidbody target)
        {
            float distAway = obstacle.Radius + distanceFromBoundary;

            Vector3 dir = obstacle.Position - target.Position;
            dir.Normalize();

            return obstacle.Position + dir * distAway;
        }

        Vector3 GetHidingPosition(MovementAIRigidbody obstacle, Transform target)
        {
            float distAway = obstacle.Radius + distanceFromBoundary;

            Vector3 dir = obstacle.Position - target.position;
            dir = new Vector3(dir.x, 0.0f, dir.z).normalized;

            return obstacle.Position + dir * distAway;
        }
    }
}