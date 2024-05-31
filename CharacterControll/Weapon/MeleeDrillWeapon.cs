using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDrillWeapon : Weapon
{
    [Header("DelayAttack")]
    [SerializeField] private float attackDelay = 0f;
    [SerializeField] private int delayAttackIndex = 0;
    [SerializeField] private float multipleDamage = 1f;
    private Character playerCharacter;
    private float InitialDamage = 0f;
    float attackBuffer = 0f;
    public override void Start()
    {
        base.Start();
        InitialDamage = weaponDamage;
    }
    public override IEnumerator PreparingAttack()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        weaponDamage = InitialDamage;
        if (playerCharacter.characterWeaponAttack.CurrentComboAttackCount == delayAttackIndex)
        {
            attackBuffer = attackDelay;
            weaponDamage *= multipleDamage;
        }
        else
        {
            attackBuffer = preparingAttackBuffer;
        }

        yield return new WaitForSeconds(attackBuffer);

        UnderAttackEvent();
        coroutine = StartCoroutine(UnderAttack());
    }
}
