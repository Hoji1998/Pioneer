using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiWheelDash : AiAbility
{
    [Header("WheelDash Component")]
    [SerializeField] private float dashForce = 10f;
    private bool IsDash = false;

    [Header("CurveModel")]
    [SerializeField] private AiCurvedPatrol curvedPatrol;

    [Header("Effect")]
    [SerializeField] private GameObject splashEffect;
    [SerializeField] private GameObject frictionEffect;
    [SerializeField] private GameObject frictionEffectPos;

    private float dashDirection;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsDash = false;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        IsDash = false;
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        IsDash = true;
        StartCoroutine(Dash());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsDash)
        {
            return false;
        }

        if (!brain.IsEngaged)
        {
            brain.AbilityChange();
            return false;
        }
        return true;
    }
    private IEnumerator Dash()
    {
        AnimationStateChange(true);
        DashDirectionCheck();
        float curTime = 0f;

        CreateSplashEffect();
        while (curTime <= 0.3f)
        {
            yield return new WaitForFixedUpdate();
            curTime += Time.deltaTime;
        }
        splashEffect.SetActive(false);

        float acceleration = 2f;
        curTime = 0f;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            curvedPatrol.CurveModel(dashDirection);

            acceleration -= 0.03f;
            character.velocity.x = (dashForce * acceleration) * dashDirection;

            curTime += Time.deltaTime;
            if (curTime >= 0.1f)
            {
                CreatefrictionEffect();
                curTime = 0f;
            }

            if (acceleration <= 0f)
            {
                break;
            }
        }

        character.velocity.x = 0f;
        AnimationStateChange(false);
        brain.AbilityChange();
    }
    private void DashDirectionCheck()
    {
        if (character.transform.position.x - playerCharacter.transform.position.x < 0)
        {
            dashDirection = 1f;
            character.transform.localScale = Vector3.one;
        }
        else
        {
            dashDirection = -1f;
            character.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
    private void CreatefrictionEffect()
    {
        GameObject instanceEffect = Instantiate(frictionEffect, frictionEffectPos.transform);
        instanceEffect.transform.parent = null;
        instanceEffect.transform.rotation = curvedPatrol.CurveObject.transform.rotation;
    }
    private void CreateSplashEffect()
    {
        splashEffect.SetActive(true);
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
