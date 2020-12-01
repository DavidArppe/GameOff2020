using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseController : MonoBehaviour
{
    FMODUnity.StudioEventEmitter musicEmitter;

    public float radius = 500.0f;

    private void Start()
    {
        musicEmitter = Camera.main.GetComponent<FollowCam>().mainMusicEmitter;
    }

    private void Update()
    {
        GlobalControl.isInBattle = 0.0f;
    }

    private void LateUpdate()
    {
        float dist = Vector3.Distance(GlobalControl.playerStatic.transform.position, transform.position);

        GlobalControl.isInBattle = Mathf.Max(GlobalControl.isInBattle, Mathf.Clamp01(Mathf.InverseLerp(radius + 200.0f, radius - 100.0f, dist)));
        musicEmitter.SetParameter("is_in_battle", GlobalControl.isInBattle);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.color = Color.red * new Color(1.0f, 1.0f, 1.0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, radius + 200.0f);
    }
}
