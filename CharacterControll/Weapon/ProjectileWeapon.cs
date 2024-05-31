using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [Header("ProjectileWeapon Component")]
    public GameObject projectile;
    public int projectileSize = 10;

    [Header("ContinuousFire Component")]
    public bool IsContinuousFire = false;
    public int maxContinuousCount = 1;

    [HideInInspector] public List<Projectile> projectiles;

    private int currentContinuousCount = 0;
    public override void Start()
    {
        comboAttackBuffer = 1f;
        InitializedAttackState();
        CreateProjectilePool();
    }
    public void CreateProjectilePool()
    {
        projectiles = new List<Projectile>();
        for (int i = 0; i < projectileSize; i++)
        {
            GameObject instanceObject = Instantiate(projectile);
            instanceObject.SetActive(false);
            projectiles.Add(instanceObject.GetComponent<Projectile>());
        }
    }
    public override void InitializedAttackState()
    {
        if (characterWeaponAttack != null)
        {
            characterWeaponAttack.AnimationStateChange(false);
        }
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        base.WeaponStateChange(WeaponState.Ready);
        currentContinuousCount = 0;
    }
    public override IEnumerator UnderAttack()
    {
        yield return new WaitForSeconds(underAttackBuffer);

        if (IsContinuousFire && (currentContinuousCount + 1 < maxContinuousCount))
        {
            currentContinuousCount++;
            base.OperateAttack();
        }
        else
        {
            base.AttackDoneEvent();
            coroutine = StartCoroutine(base.AttackDone());
        }
    }
    public override void UnderAttackEvent()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return;
        }
        LaunchOfProjectile();
        base.WeaponStateChange(WeaponState.UnderAttack);
    }
    public void LaunchOfProjectile()
    {
        foreach (Projectile instanceProjectile in projectiles)
        {
            if (!instanceProjectile.gameObject.activeSelf)
            {
                instanceProjectile.damage = weaponDamage;
                instanceProjectile.targetDirection = LevelManager.Instance.playerCharacter.transform.position - transform.position;
                instanceProjectile.LaunchOfProjectile(transform.position);
                instanceProjectile.gameObject.SetActive(true);
                break;
            }
        }
    }
}
