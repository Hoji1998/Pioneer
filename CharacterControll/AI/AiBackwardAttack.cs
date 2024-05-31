using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiBackwardAttack : AiAbility
{
    [Header("BackwardAttack Component")]
    [SerializeField] private float backWardForce = 10f;
    private bool IsBackwardAttack = false;
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
        IsBackwardAttack = false;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        IsBackwardAttack = false;
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
        IsBackwardAttack = true;
        brain.UpdateBrainWeight();
        StartCoroutine(BackwardAttack());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsBackwardAttack)
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
        //    IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x < 0 ? false : true;
        //}
        //else
        //{
        //    IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x >= 0 ? false : true;
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
    private IEnumerator BackwardAttack()
    {
        AnimationStateChange(true);
        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        while (curTime <= 0.5f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
        character.animator.SetTrigger("RushAttack_UnderAttack");
        float rushDirection = character.transform.localScale.x > 0 ? -1 : 1;
        float acceleration = 1f;
        curTime = 0f;
        //float accelerationCoefficient = 0.02f;
        while (true)
        {
            yield return loop;
            acceleration -= 0.015f;
            //accelerationCoefficient += 0.005f;
            character.velocity.x = (backWardForce * acceleration) * rushDirection;

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
        instanceEffect.transform.localScale = new Vector3(this.gameObject.transform.localScale.x * -1f, 1f, 1f);
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
