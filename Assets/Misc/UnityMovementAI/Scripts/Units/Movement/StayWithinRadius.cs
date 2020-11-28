using UnityEngine;

namespace UnityMovementAI
{
    public class StayWithinRadius : MonoBehaviour
    {
        public Transform pointToStayNear;

        public bool useTargetScaleAsDistance = true;
        public float distanceFromPoint = 10.0f;
        public float featherDistance = 10.0f;

        public float power;

        SteeringBasics steeringBasics;
        CollisionAvoidance colAvoid;
        NearSensor colAvoidSensor;

        void Start()
        {
            steeringBasics = transform.parent.GetComponent<SteeringBasics>();
            colAvoid = transform.parent.GetComponent<CollisionAvoidance>();
            colAvoidSensor = transform.parent.Find("ColAvoidSensor").GetComponent<NearSensor>();

            if (useTargetScaleAsDistance)
            {
                float maxS = Mathf.Max(pointToStayNear.transform.localScale.x, Mathf.Max(pointToStayNear.transform.localScale.y, pointToStayNear.transform.localScale.z));
                pointToStayNear.transform.localScale = new Vector3(maxS, maxS, maxS);
            }
        }

        private void OnValidate()
        {
            if (useTargetScaleAsDistance)
            {
                float maxS = Mathf.Max(pointToStayNear.localScale.x, Mathf.Max(pointToStayNear.localScale.y, pointToStayNear.localScale.z));
                pointToStayNear.localScale = new Vector3(maxS, maxS, maxS);
            }
        }

        void FixedUpdate()
        {
            Vector3 accel = colAvoid.GetSteering(colAvoidSensor.targets);

            if (accel.magnitude < 0.005f)
            {
                var dist = useTargetScaleAsDistance ? pointToStayNear.localScale.x : distanceFromPoint;
                var vecFromPoint = (pointToStayNear.position - transform.position);
                power = Mathf.Clamp01(Mathf.InverseLerp(dist, dist + featherDistance, vecFromPoint.magnitude)) * 10.0f;
                accel = vecFromPoint.normalized * power;
            }

            steeringBasics.Steer(accel);
            steeringBasics.LookWhereYoureGoing();
        }

        private void OnDrawGizmos()
        {
            var dist = useTargetScaleAsDistance ? pointToStayNear.localScale.x : distanceFromPoint;

            Gizmos.color = new Color(1.0f, 1.0f, 0.4f, 0.1f);
            Gizmos.DrawWireSphere(pointToStayNear.position, dist);
            Gizmos.color = new Color(0.4f, 0.4f, 1.0f, 0.2f);
            Gizmos.DrawWireSphere(pointToStayNear.position, dist + featherDistance);
        }
    }
}