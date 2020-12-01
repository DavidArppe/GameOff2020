using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform gunBarrelTrans;
    public Transform gunPivotTrans;

    public LayerMask raycastMask;

    public float health = 100.0f;

    public TMPro.TextMeshProUGUI mode;
    public TMPro.TextMeshProUGUI shields;

    [FMODUnity.EventRef]
    public string shootSound;

    public float shootTimer = 0.0f;

    public ParticleSystem muzzleFlash;
    public ParticleSystem[] muzzleFlashesJet;
    int shootIndex = 0;
    public GameObject lineRendererPrefab;

    void Update()
    {
        AimWeapon();

        shields.text = string.Format("SHIELDS: {0}%", ((int)health).ToString().PadLeft(3, ' '));
        mode.text = string.Format("MODE: {0}",
            VehicleTypeSwitch.instance.HasVehicleBit(VehicleTypeSwitch.VehicleType.JET) ? "JET" :
            VehicleTypeSwitch.instance.HasVehicleBit(VehicleTypeSwitch.VehicleType.HOVER) ? "HOVERTANK" : "CAR");

        if (VehicleTypeSwitch.instance.HasVehicleBit(VehicleTypeSwitch.VehicleType.JET))
        {
            if (UnityInputModule.instance.controls.Player.Fire2.ReadValue<float>() > 0.5f && shootTimer < 0.0f)
            {
                shootTimer = 0.15f;
                FMODUnity.RuntimeManager.PlayOneShot(GlobalControl.instance.shootEventSound, transform.position);

                Transform muzzleTransform = muzzleFlashesJet[shootIndex].transform;
                muzzleFlashesJet[shootIndex].Play(true);

                Vector3 target = muzzleTransform.position + muzzleTransform.forward * 10000.0f;
                AIReceiver hitEnemy = null;
                if (Physics.Raycast(new Ray(muzzleTransform.position, muzzleTransform.forward), out RaycastHit info, 10000.0f, raycastMask))
                {
                    target = info.point;
                    hitEnemy = info.collider.GetComponentInParent<AIReceiver>();

                    if (hitEnemy != null)
                    {
                        hitEnemy.Damage(25.0f);
                    }
                }

                // bullet Trail
                var bulletTrail = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
                var lr = bulletTrail.GetComponent<LineRenderer>();
                lr.positionCount = (2);
                lr.SetPositions(new Vector3[] { muzzleTransform.position, target });

                shootIndex = (++shootIndex) % 2;
            }
        }
        else
        {
            if (UnityInputModule.instance.controls.Player.Fire.ReadValue<float>() > 0.5f && shootTimer < 0.0f)
            {
                shootTimer = 0.3f;
                FMODUnity.RuntimeManager.PlayOneShot(GlobalControl.instance.shootEventSound, transform.position);

                Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * 10000.0f;
                AIReceiver hitEnemy = null;
                if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out RaycastHit info, 10000.0f, raycastMask))
                {
                    target = info.point;
                    hitEnemy = info.collider.GetComponentInParent<AIReceiver>();

                    if (hitEnemy != null)
                    {
                        hitEnemy.Damage(25.0f);
                    }
                }

                // Muzzle flash
                Transform muzzleTransform = muzzleFlash.transform;
                muzzleFlash.Play(true);

                // bullet Trail
                var bulletTrail = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
                var lr = bulletTrail.GetComponent<LineRenderer>();
                lr.positionCount = (2);
                lr.SetPositions(new Vector3[] { muzzleTransform.position, target });
            }
        }

        shootTimer -= Time.deltaTime;
    }

    void AimWeapon()
    {
        Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * 10000.0f;

        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out RaycastHit info, 10000.0f, raycastMask))
        {
            target = info.point;
        }

        gunPivotTrans.LookAt(gunPivotTrans.position + Vector3.ProjectOnPlane(target - gunPivotTrans.position, transform.up), transform.up);
        gunBarrelTrans.LookAt(gunBarrelTrans.position + Vector3.ProjectOnPlane(target - gunBarrelTrans.position, gunPivotTrans.right), transform.up);
    }

    public void Damage(float damage)
    {
        health -= damage;

        if (health <= 0.0f)
        {
            UnityEditor.SceneManagement.LoadScene()
        }
    }
}
