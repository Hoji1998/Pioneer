using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectileWeapon : ProjectileWeapon
{
    public override void UnderAttackEvent()
    {
        foreach (Projectile instanceProjectile in projectiles)
        {
            if (!instanceProjectile.gameObject.activeSelf)
            {
                instanceProjectile.damage = weaponDamage;
                instanceProjectile.targetDirection = LevelManager.Instance.playerCharacter.transform.localScale.x < 0 ? Vector2.left : Vector2.right;
                instanceProjectile.LaunchOfProjectile(transform.position);
                instanceProjectile.gameObject.SetActive(true);
                break;
            }
        }
        WeaponStateChange(WeaponState.UnderAttack);
    }
}
