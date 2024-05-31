using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedHealth : MonoBehaviour
{
    [Header("SharedHealth Component")]
    [SerializeField] private Health health;

    [HideInInspector] public Health OriginalHealth { get => health; }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (health.currentHealth <= 0)
            return;

        Weapon weapon = collision.GetComponent<Weapon>() == null ? collision.GetComponentInParent<Weapon>() : collision.GetComponent<Weapon>();

        WeaponCollideCheck(weapon);
    }
    private void WeaponCollideCheck(Weapon weapon)
    {
        if (weapon == null)
            return;

        health.DamageCalculate(weapon.weaponDamage);
    }
}
