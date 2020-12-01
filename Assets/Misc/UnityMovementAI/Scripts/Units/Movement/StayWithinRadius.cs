using UnityEngine;

namespace UnityMovementAI
{
    public class StayWithinRadius : MonoBehaviour
    {
        Vector3 pointToStayNear;

        float distanceFromPoint = 10.0f;
        float featherDistance = 40.0f;
        float power = 20.0f;

        SteeringBasics steeringBasics;
        CollisionAvoidance colAvoid;
        NearSensor colAvoidSensor;

        void Start()
        {
            steeringBasics = transform.parent.GetComponent<SteeringBasics>();
            colAvoid = transform.parent.GetComponent<CollisionAvoidance>();
            colAvoidSensor = transform.parent.Find("ColAvoidSensor").GetComponent<NearSensor>();

            var baseParent = GetComponentInParent<EnemyBaseController>();
            pointToStayNear = baseParent.transform.position;
            distanceFromPoint = baseParent.radius;
        }

        void FixedUpdate()
        {
            Vector3 accel = colAvoid.GetSteering(colAvoidSensor.targets);

            if (accel.magnitude < 0.005f)
            {
                var dist = distanceFromPoint;
                var vecFromPoint = (pointToStayNear - transform.position);
                power = Mathf.Clamp01(Mathf.InverseLerp(dist, dist + featherDistance, vecFromPoint.magnitude)) * 10.0f;
                accel = vecFromPoint.normalized * power;
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }

        private void OnDrawGizmos()
        {
            var dist = distanceFromPoint;

            Gizmos.color = new Color(1.0f, 1.0f, 0.4f, 0.1f);
            Gizmos.DrawWireSphere(pointToStayNear, dist);
            Gizmos.color = new Color(0.4f, 0.4f, 1.0f, 0.2f);
            Gizmos.DrawWireSphere(pointToStayNear, dist + featherDistance);
        }
    }
}