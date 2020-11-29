using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIReceiver : MonoBehaviour
{
    public float Health = 100.0f;

    public GameObject wanderChild;
    public GameObject hideChild;
    public GameObject stayNearPoint;

    private UnityMovementAI.SteeringBasics steering;

    private void Start()
    {
        steering = GetComponent<UnityMovementAI.SteeringBasics>();
    }

    public void SwitchToWanderMode()
    {
        hideChild.SetActive(false);

        wanderChild.SetActive(true);
    }

    public void SwitchToHideMode()
    {
        wanderChild.SetActive(false);

        hideChild.SetActive(true);
    }

    public void StopMovement()
    {
        wanderChild.SetActive(false);
        hideChild.SetActive(false);

        steering.Arrive(transform.position);
    }
}