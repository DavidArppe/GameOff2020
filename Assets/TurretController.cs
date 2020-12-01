using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public float distanceUntilShooting = 500.0f;
    public Transform gunBarrelTrans;
    public Transform gunPivotTrans;

    public bool isRegularGun = true;

    [Header("Traditional Gun")]
    public ParticleSystem[] muzzleFlashes;
    int shootIndex = 0;
    public GameObject lineRendererPrefab;

    [Header("Laser Gun")]
    public GameObject laserRendererPrefab;

    public float shootingTimer = 1.0f;
    float timer = 0.0f;

    public float lookSpeed = 1.0f;

    void SmoothLookAt(Transform t, Vector3 target, Vector3 up)
    {
        Vector3 direction = target - t.position;
        Quaternion toRotation = Quaternion.LookRotation(direction, up);
        t.rotation = Quaternion.Lerp(t.rotation, toRotation, lookSpeed * Time.deltaTime);
    }

    void Update()
    {
        Vector3 target = GlobalControl.playerStatic.transform.position;

        if (Vector3.Distance(target, transform.position) < distanceUntilShooting)
        {

            //gunPivotTrans.LookAt(gunPivotTrans.position + Vector3.ProjectOnPlane(target - gunPivotTrans.position, transform.up), transform.up);
            //gunBarrelTrans.LookAt(gunBarrelTrans.position + Vector3.ProjectOnPlane(target - gunBarrelTrans.position, gunPivotTrans.right), transform.up);

            SmoothLookAt(gunPivotTrans, gunPivotTrans.position + Vector3.ProjectOnPlane(target - gunPivotTrans.position, transform.up), transform.up);
            SmoothLookAt(gunBarrelTrans, gunBarrelTrans.position + Vector3.ProjectOnPlane(target - gunBarrelTrans.position, gunPivotTrans.right), transform.up);

            if (timer <= 0.0f)
            {
                if (Vector3.Dot((target - gunBarrelTrans.position).normalized, gunBarrelTrans.forward) > 0.998f)
                {
                    timer = shootingTimer;
                    Shoot();
                }
            }
    
            timer -= Time.deltaTime;
        }
    }
    
    void Shoot()
    {

        if (isRegularGun)
        {
            // Muzzle flash
            Transform muzzleTransform = muzzleFlashes[shootIndex].transform;
            muzzleFlashes[shootIndex].Play(true);

            // Raycast
            Vector3 target = muzzleTransform.position + muzzleTransform.forward * distanceUntilShooting;
            if (Physics.Raycast(new Ray(muzzleTransform.position, muzzleTransform.forward), out RaycastHit info, distanceUntilShooting))
            {
                target = info.point;
                var wc = info.collider.gameObject.GetComponentInParent<WeaponController>();

                if (wc != null)
                {
                    wc.Damage(GlobalControl.turretDamageAmount);
                }
            }

            // bullet Trail
            var bulletTrail = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
            var lr = bulletTrail.GetComponent<LineRenderer>();
            lr.positionCount = (2);
            lr.SetPositions(new Vector3[] { muzzleTransform.position, target });

            shootIndex = (++shootIndex) % 2;

            FMODUnity.RuntimeManager.PlayOneShot(GlobalControl.instance.shootEventSound, transform.position);
        }
        else
        {
            // Raycast
            Vector3 target = gunBarrelTrans.position + gunBarrelTrans.forward * distanceUntilShooting;
            if (Physics.Raycast(new Ray(gunBarrelTrans.position, gunBarrelTrans.forward), out RaycastHit info, distanceUntilShooting))
            {
                target = info.point;
                var wc = info.collider.gameObject.GetComponentInParent<WeaponController>();

                if (wc != null)
                {
                    wc.Damage(GlobalControl.laserDamageAmount);
                }
            }

            // Laser
            var laser = Instantiate(laserRendererPrefab, Vector3.zero, Quaternion.identity);
            var lr = laser.GetComponent<LineRenderer>();
            lr.positionCount = (2);
            lr.SetPositions(new Vector3[] { gunBarrelTrans.position, target });


            FMODUnity.RuntimeManager.PlayOneShot(GlobalControl.instance.laserEventSound, transform.position);
        }
    }
}
