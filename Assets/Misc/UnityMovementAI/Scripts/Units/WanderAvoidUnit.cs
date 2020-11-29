using UnityEngine;

namespace UnityMovementAI
{
    public class WanderAvoidUnit : MonoBehaviour
    {
        SteeringBasics steeringBasics;
        Wander2 wander;
        CollisionAvoidance colAvoid;

        NearSensor colAvoidSensor;

        void Start()
        {
            steeringBasics = transform.parent.GetComponent<SteeringBasics>();
            wander = GetComponent<Wander2>();
            colAvoid = transform.parent.GetComponent<CollisionAvoidance>();

            colAvoidSensor = transform.parent.Find("ColAvoidSensor").GetComponent<NearSensor>();
        }

        void FixedUpdate()
        {
            Vector3 accel = colAvoid.GetSteering(colAvoidSensor.targets);

            if (accel.magnitude < 0.005f)
            {
                accel = wander.GetSteering();
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }
    }
}