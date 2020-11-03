using UnityEngine;

public interface IWeapon : ITeamFiltered
{
    float TimeLastFired { get; }
    float FireRate { get; }
    float ProjectileSpeed { get; }
    float WeaponRange { get; }
    float Damage { get; }
    float Speed { get; }

    IEntity Parent { get; set; }
    IProjectile Projectile { get; }

    void Fire();
}
