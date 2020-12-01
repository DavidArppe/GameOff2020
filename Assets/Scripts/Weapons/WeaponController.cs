using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform gunBarrelTrans;
    public Transform gunPivotTrans;

    void Update()
    {
        AimWeapon();    
    }

    void AimWeapon()
    {
        Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * 10000.0f;

        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f)), out RaycastHit info))
        {
            target = info.point;
        }

        gunPivotTrans.LookAt(gunPivotTrans.position + Vector3.ProjectOnPlane(target - gunPivotTrans.position, transform.up), transform.up);
        gunBarrelTrans.LookAt(gunBarrelTrans.position + Vector3.ProjectOnPlane(target - gunBarrelTrans.position, gunPivotTrans.right), transform.up);
    }
}
