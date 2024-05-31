using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDropProjectile : AiAbility
{
    [Header("DropProjectile Component")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackXrange = 1f;
    [SerializeField] private GameObject weapon;
    [SerializeField] private Transform weaponTransform;
    [Header("Fly Component")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private Vector3 distancingPoint;

    private Vector2 direction = Vector2.zero;
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
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        CalculateDirection();

        character.velocity += direction.normalized * speed * Time.deltaTime;
        LimitSpeedCheck();

        if (CheckAttackRange())
        {
            DropProjectile();
        }
    }
    private void LimitSpeedCheck()
    {
        if (character.velocity.x < -speed)
        {
            character.velocity.x = -speed;
        }
        else if (character.velocity.x > speed)
        {
            character.velocity.x = speed;
        }

        if (character.velocity.y < -speed)
        {
            character.velocity.y = -speed;
        }
        else if (character.velocity.y > speed)
        {
            character.velocity.y = speed;
        }
    }
    private bool calculateValue = false;
    private void CalculateDirection()
    {
        switch (brain.usePlayerSearchField)
        {
            case true:
                calculateValue = brain.IsSearchCharacter ? true : false;
                break;
            case false:
                calculateValue = brain.IsEngaged ? true : false;
                break;
        }

        switch (calculateValue)
        {
            case true:
                direction = playerCharacter.transform.position.x - transform.position.x < 0
                    ? (playerCharacter.transform.position + distancingPoint) - transform.position
                    : (playerCharacter.transform.position + new Vector3(distancingPoint.x * -1f, distancingPoint.y, 0f)) - transform.position;
                character.FlipCharacter(playerCharacter.transform.position.x - transform.position.x);
                break;
            case false:
                direction = brain.initializeTr - transform.position;
                character.FlipCharacter(direction.normalized.x);
                break;
        }
    }

    private bool CheckAttackRange()
    {
        if (playerCharacter == null)
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();

        if (playerCharacter.transform.position.y > character.transform.position.y)
            return false;

        if (Mathf.Abs(playerCharacter.transform.position.x - character.transform.position.x) > attackXrange)
            return false;

        return true;
    }
    private void DropProjectile()
    {
        currentWeapon.OperateAttack();
        IsAttack = true;
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
        return true;
    }
    private IEnumerator AttackStart()
    {
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
