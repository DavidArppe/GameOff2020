public interface IProjectile
{
    float BlastRadius { get; }
    bool HitScan { get; } // TODO - Turn into enum for projectile/raycast
    int FragCount { get; }
    float BaseDamage { get; }
    IWeapon FromWeapon { get; set; }
}