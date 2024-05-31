using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatelliteLaserWeapon : Weapon
{
    [Header("ProjectileWeapon Component")]
    public GameObject projectile;
    public int projectileSize = 10;

    [Header("MultipleShot")]
    [SerializeField] private int multiShotSize = 5;
    [SerializeField] private float multiShotRange = 5f;

    [Header("ContinuousFire Component")]
    public bool IsContinuousFire = false;
    public int maxContinuousCount = 1;

    [HideInInspector] public List<Projectile> projectiles;

    private int currentContinuousCount = 0;
    private Character playerCharacter;
    private Coroutine chasingShotCoroutine;
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
        ChooseShotMode();

        base.WeaponStateChange(WeaponState.UnderAttack);
    }
    public void LaunchOfProjectile(Vector3 spawnTr)
    {
        foreach (Projectile instanceProjectile in projectiles)
        {
            if (!instanceProjectile.gameObject.activeSelf)
            {
                instanceProjectile.damage = weaponDamage;
                instanceProjectile.LaunchOfProjectile(spawnTr);
                instanceProjectile.gameObject.SetActive(true);
                break;
            }
        }
    }
    private void ChooseShotMode()
    {
        LaunchOfChasingShot();
        //float randomSeed = Random.Range(-1f, 1f);
        //if (randomSeed <= 0)
        //    LaunchOfChasingShot();
        //else
        //    LaunchOfMultiShot();

    }
    private void LaunchOfMultiShot()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        Vector3 currentSpawnTr = new Vector3(playerCharacter.transform.position.x - (multiShotRange * (multiShotSize / 2)), transform.position.y, 0f);
        for (int i = 0; i < multiShotSize; i++)
        {
            LaunchOfProjectile(currentSpawnTr);
            currentSpawnTr.x += multiShotRange;
        }
    }
    private void LaunchOfChasingShot()
    {
        chasingShotCoroutine = StartCoroutine(ChasingShotStart());
    }
    private IEnumerator ChasingShotStart()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        float randomSeed = Random.Range(-1f, 1f);
        Vector3 currentSpawnTr = randomSeed <= 0 ?
            new Vector3(playerCharacter.transform.position.x - (0.8f * (multiShotSize / 2)), transform.position.y, 0f) :
            new Vector3(playerCharacter.transform.position.x + (0.8f * (multiShotSize / 2)), transform.position.y, 0f);

        var loop = new WaitForSeconds(0.05f);
        int currentShotCount = 0;

        while (currentShotCount <= multiShotSize)
        {
            yield return loop;
            LaunchOfProjectile(currentSpawnTr);
            currentSpawnTr.x += randomSeed <= 0 ? 0.8f : -0.8f;
            currentShotCount++;
        }
    }
}
