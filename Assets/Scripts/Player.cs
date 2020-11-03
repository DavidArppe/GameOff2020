
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IEntity
{
    [field: SerializeField]
    public bool IsDead { get; set; }

    [field: SerializeField]
    public float Health { get; set; }

    [field: SerializeField]
    public float Shield { get; set; }

    public IList<IWeapon> ActiveWeapons { get; set; }

    public Team Team => Team.Player;

    private Rigidbody rigidBody;

    public GameObject Weapon;

    public Rigidbody RigidBody => rigidBody;

    private void Awake()
    {
        ActiveWeapons = new List<IWeapon>();
        rigidBody = GetComponent<Rigidbody>();
        ActiveWeapons.Add(Weapon.GetComponent<IWeapon>());
        foreach (var w in ActiveWeapons)
        {
            w.Parent = this;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (UnityInputModule.instance.controls.Player.Fire.ReadValue<float>() > 0.0f)
        {
            foreach (var weapon in ActiveWeapons)
            {
                weapon.Fire();
            }
        }
    }
}