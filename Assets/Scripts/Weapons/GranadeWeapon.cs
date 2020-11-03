using UnityEngine;

public class GranadeWeapon : MonoBehaviour, IWeapon
{
    public float TimeLastFired { get; private set; } = 0;

    [field: SerializeField]
    public float FireRate { get; set; } = 2;

    [field: SerializeField]
    public float ProjectileSpeed { get; set; } = 10;

    [field: SerializeField]
    public float WeaponRange { get; set; } = 10;

    [field: SerializeField]
    public float Damage { get; set; } = 10;

    [field: SerializeField]
    public float Speed { get; set; } = 1;

    [field: SerializeField]
    public IEntity Parent { get; set; }

    public Team Team => Parent.Team;

    public IProjectile Projectile { get; set; }

    public GameObject ProjectlePrefab;

    private Camera cam;
    private Transform camTransform;

    private void Awake()
    {
        cam = Camera.main;
        camTransform = cam.transform;
        Projectile = ProjectlePrefab.GetComponent<IProjectile>();
    }

    public void Fire()
    {
        var proj = Instantiate(ProjectlePrefab, Parent.RigidBody.position + new Vector3(0.0f, 1.0f, 0.0f), Parent.RigidBody.rotation);
        var projRigidBody = proj.GetComponent<Rigidbody>();
        var projProjectle = proj.GetComponent<IProjectile>();
        projProjectle.FromWeapon = this;

        projRigidBody.velocity = Parent.RigidBody.velocity;
        // projRigidBody.angularVelocity = ;
        projRigidBody.AddForce(transform.forward * ProjectileSpeed, ForceMode.Impulse);
    }
}