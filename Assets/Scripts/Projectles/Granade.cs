using UnityEngine;

public class Granade : MonoBehaviour, IProjectile, ITeamFiltered
{
    public float BlastRadius => 3;
    public bool HitScan => false;
    public int FragCount => 1;
    public float BaseDamage => 20;
    public float AliveTime => 5;

    public IWeapon FromWeapon { get; set; }

    public Team Team => FromWeapon.Team;

    private float aliveTime = 0.0f;

    void Start()
    {

    }

    void Update()
    {
        aliveTime += Time.deltaTime;

        if (aliveTime > AliveTime)
        {
            Destroy(gameObject);
        }
    }
}