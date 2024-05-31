using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDrillAttack : AiAbility
{
    private bool IsAttack = false;
    private bool IsAttackDetect = false;

    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsAttack = false;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
    }
    public override void ProcessAbility()
    {
        DetectCheck();
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        IsAttack = true;
        StartCoroutine(DrillAttack());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsAttack)
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

        if (character.transform.localScale.x > 0)
        {
            IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x < 0 ? true : false;
        }
        else
        {
            IsAttackDetect = character.transform.position.x - playerCharacter.transform.position.x >= 0 ? true : false;
        }
    }
    private IEnumerator DrillAttack()
    {
        AnimationStateChange(true);
        //yield return new WaitForSeconds(0.5f);

        character.animator.SetTrigger(changeAnimationName + "UnderAttack");

        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        while (curTime <= 3.083f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }
        character.animator.SetTrigger(changeAnimationName + "AttackDone");

        curTime = 0f;
        while (curTime <= 1f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }
        AnimationStateChange(false);
        IsAttack = false;
        brain.AbilityChange();
    }
    public override void AnimationStateChange(bool changeValue)
    {
        if (character.animator == null)
            return;

        character.animator.SetBool(changeAnimationName + "PreparingAttack", changeValue);

        if (changeValue)
        {
            brain.currentAiState = AiBrain.AiState.Proceeding;
        }
    }
}
