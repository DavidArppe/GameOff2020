using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIReceiver : MonoBehaviour
{
    public GameObject wanderChild;
    public GameObject hideChild;
    public GameObject stayNearPoint;

    private UnityMovementAI.SteeringBasics steering;

    public GameObject explosion;
    [FMODUnity.EventRef]
    public string explosionEvent;

    public GameObject toDestroyWhenBlownUp;

    public float health = 75.0f;

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

    public void Damage(float damage)
    {
        health -= damage;

        if (health < 0.0f && toDestroyWhenBlownUp != null)
        {
            Instantiate(explosion, toDestroyWhenBlownUp.transform.position, Quaternion.identity);
            FMODUnity.RuntimeManager.PlayOneShot(explosionEvent, transform.position);
            Destroy(toDestroyWhenBlownUp);
            toDestroyWhenBlownUp = null;
        }
    }
}