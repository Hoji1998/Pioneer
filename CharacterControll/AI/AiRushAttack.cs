using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AiRushAttack : AiAbility
{
    [Header("RushAttack Component")]
    [SerializeField] private float rushForce = 10f;
    private bool IsRushAttack = false;
    private bool IsAttackDetect = false;

    [Header("Effect")]
    [SerializeField] private GameObject splashEffect;
    [SerializeField] private GameObject splashEffectPos;
    [SerializeField] private GameObject frictionEffect;
    [SerializeField] private GameObject frictionEffectPos;

    [Header("Connection Ability")]
    [SerializeField] private AiAbility connectionAbility;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsRushAttack = false;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        IsRushAttack = false;
    }
    public override void ProcessAbility()
    {
        DetectCheck();
        WallCheck();

        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        IsRushAttack = true;
        brain.UpdateBrainWeight();
        StartCoroutine(RushAttack());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsRushAttack)
        {
            return false;
        }

        if (!brain.IsEngaged || !IsAttackDetect)
        {
            brain.AbilityChange();
            return false;
        }
        return true;
    }
    private void DetectCheck()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        //if (character.transform.localScale.x > 0)
        //{
        //    IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x < 0 ? true : false;
        //}
        //else
        //{
        //    IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x >= 0 ? true : false;
        //}

        IsAttackDetect = true;
    }
    private void WallCheck()
    {
        if (character.directionCollideState.leftWall || character.directionCollideState.rightWall)
        {
            playerCharacter.movingPlatformVelocity = Vector2.zero;
            return;
        }

        playerCharacter.movingPlatformVelocity = character.velocity;
    }
    private IEnumerator RushAttack()
    {
        AnimationStateChange(true);
        float curTime = 0f;
        float rushDirection = character.transform.localScale.x > 0 ? 1 : -1;
        float acceleration = 1f;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            character.velocity.x = (acceleration * 3f) * (rushDirection * -1f);
            acceleration -= 0.03f;

            curTime += Time.deltaTime;
            if (acceleration <= 0.06f)
            {
                break;
            }
        }

        character.animator.SetTrigger("RushAttack_UnderAttack");
        acceleration = 2f;
        curTime = 0f;
        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
        while (true)
        {
            yield return new WaitForFixedUpdate();
            acceleration -= 0.06f;
            character.velocity.x = (rushForce * acceleration) * rushDirection;

            curTime += Time.deltaTime;
            if (curTime >= 0.1f)
            {
                if (acceleration <= 0.5f)
                {
                    CreatefrictionEffect();
                }
                CreateSplashEffect();
                curTime = 0f;
            }

            if (acceleration <= 0f)
            {
                break;
            }
        }
        character.animator.SetTrigger("RushAttack_AttackDone");
        character.velocity.x = 0f;

        //yield return new WaitForSeconds(1f);
        AnimationStateChange(false);
        connectionAbility.AbilityStart();
    }
    private void CreatefrictionEffect()
    {
        GameObject instanceEffect = Instantiate(frictionEffect);
        instanceEffect.transform.position = frictionEffectPos.transform.position;
        instanceEffect.transform.rotation = frictionEffectPos.transform.rotation;
        instanceEffect.transform.localScale = this.gameObject.transform.localScale;
    }
    private void CreateSplashEffect()
    {
        GameObject instanceEffect = Instantiate(splashEffect);
        instanceEffect.transform.position = splashEffectPos.transform.position;
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
