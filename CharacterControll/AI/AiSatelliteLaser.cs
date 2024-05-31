using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiSatelliteLaser : AiAbility
{
    [Header("AiSatelliteLaser Component")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform weaponTransform;

    private Weapon currentWeapon;
    private GameObject characterWeapon;
    private bool IsAttack = false;
    private Coroutine coroutine;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
        if (characterWeapon != null)
        {
            Destroy(characterWeapon);
        }
        characterWeapon = Instantiate(weapon);
        characterWeapon.transform.parent = character.gameObject.transform;
        characterWeapon.transform.localPosition = weaponTransform.localPosition;
        characterWeapon.transform.rotation = weaponTransform.rotation;

        currentWeapon = characterWeapon.GetComponent<Weapon>();
        currentWeapon.weaponDamage = damage;
        currentWeapon.character = character;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        IsAttack = false;
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
        {
            return;
        }

        AbilityStart();
    }
    public override void AbilityStart()
    {
        currentWeapon.OperateAttack();
        IsAttack = true;
        brain.UpdateBrainWeight();
        coroutine = StartCoroutine(AttackStart());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned
            || currentWeapon.currentWeaponState == Weapon.WeaponState.PreparingAttack
            || currentWeapon.currentWeaponState == Weapon.WeaponState.UnderAttack
            || IsAttack)
        {
            return false;
        }

        //if (!brain.IsEngaged)
        //{
        //    brain.AbilityChange();
        //    return false;
        //}
        return true;
    }
    private IEnumerator AttackStart()
    {
        ChasingFlip();
        while (true)
        {
            yield return null;
            if (character.characterCondition == Character.CharacterCondition.Stunned)
            {
                character.animator.SetBool(changeAnimationName + "PreparingAttack", false);
                character.animator.SetBool(changeAnimationName + "UnderAttack", false);
                character.animator.SetBool(changeAnimationName + "AttackDone", false);
                currentWeapon.InitializedAttackState();

                IsAttack = false;
                brain.AbilityChange();
                StopCoroutine(coroutine);
                break;
            }

            switch (currentWeapon.currentWeaponState)
            {
                case Weapon.WeaponState.PreparingAttack:
                    character.animator.SetBool(changeAnimationName + "PreparingAttack", true);
                    character.animator.SetBool(changeAnimationName + "UnderAttack", false);
                    character.animator.SetBool(changeAnimationName + "AttackDone", false);
                    break;
                case Weapon.WeaponState.UnderAttack:
                    character.animator.SetBool(changeAnimationName + "UnderAttack", true);
                    character.animator.SetBool(changeAnimationName + "PreparingAttack", false);
                    character.animator.SetBool(changeAnimationName + "AttackDone", false);
                    break;
                case Weapon.WeaponState.AttackDone:
                    character.animator.SetBool(changeAnimationName + "PreparingAttack", false);
                    character.animator.SetBool(changeAnimationName + "UnderAttack", false);
                    character.animator.SetBool(changeAnimationName + "AttackDone", true);
                    break;
                case Weapon.WeaponState.Ready:
                    character.animator.SetBool(changeAnimationName + "PreparingAttack", false);
                    character.animator.SetBool(changeAnimationName + "UnderAttack", false);
                    character.animator.SetBool(changeAnimationName + "AttackDone", false);

                    IsAttack = false;
                    brain.AbilityChange();
                    StopCoroutine(coroutine);
                    break;
            }
        }
    }
    private void ChasingFlip()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        if (character.transform.position.x - playerCharacter.transform.position.x < 0)
        {
            character.FlipCharacter(1f);
        }
        else
        {
            character.FlipCharacter(-1f);
        }
    }
    public override void AnimationStateChange(bool changeValue)
    {
        if (character.animator == null)
            return;

        character.animator.SetBool(changeAnimationName, changeValue);

        if (changeValue)
        {
            brain.currentAiState = AiBrain.AiState.Proceeding;
        }
    }
}
