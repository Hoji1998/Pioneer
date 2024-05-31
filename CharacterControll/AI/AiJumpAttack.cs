using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AiJumpAttack : AiAbility
{
    [Header("JumpAttack Component")]
    [SerializeField] private float jumpForce = 10f;
    private bool IsJumpAttack = false;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsJumpAttack = false;
        character.velocity = Vector2.zero;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        IsJumpAttack = true;
        StartCoroutine(JumpAttack());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsJumpAttack)
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
    private IEnumerator JumpAttack()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        
        character.velocity.y = jumpForce;
        AnimationStateChange(true);
        float horizontalSpeed = transform.position.x - playerCharacter.transform.position.x >= 0 ? -1f : 1f;

        while (true)
        {
            yield return null;
            character.velocity.x = horizontalSpeed * 5f;
            character.horizontalSpeed = horizontalSpeed * 5f;
            if (character.velocity.y == 0)
                break;
        }
        character.velocity.x = 0f;
        yield return new WaitForSeconds(1f);
        IsJumpAttack = false;
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
