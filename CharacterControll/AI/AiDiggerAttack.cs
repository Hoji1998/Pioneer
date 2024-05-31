using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDiggerAttack : AiAbility
{
    [Header("DiggerAttack Component")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float digSpeed = 5f;
    [SerializeField] private float digTime = 1.5f;
    private bool IsDiggerAttack = false;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        IsDiggerAttack = false;

        if (character == null)
        {
           character = GetComponent<Character>();
        }
        character.velocity = Vector2.zero;
    }
    public override void InitializedAbility()
    {
        StopAllCoroutines();
        IsDiggerAttack = false;
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
        IsDiggerAttack = true;
        StartCoroutine(Digging());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsDiggerAttack)
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
    private IEnumerator Digging()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        character.animator.SetBool("Dig", true);
        yield return new WaitForSeconds(0.667f);

        if (character.health.currentHealth <= 0)
        {
            StopAllCoroutines();
        }
        character.headbuttDamage = 0f;
        character.health.invulnerable = true;

        var loop = new WaitForFixedUpdate();
        float direction;
        Vector2 lastPlayerPosition = playerCharacter.transform.position;
        float curTime = 0f;
        bool IsCliff = false;

        while (true)
        {
            yield return loop;
            curTime += Time.deltaTime;

            if (character.directionCollideState.rightCliff
                || character.directionCollideState.leftCliff)
            {
                IsCliff = true;
            }
            
            direction = lastPlayerPosition.x - transform.position.x < 0 ? -1f : 1f;
            character.velocity.x = IsCliff ? 0f : digSpeed * direction;

            if (curTime >= digTime || Mathf.Abs(character.transform.position.x - lastPlayerPosition.x) <= 0.1f)
                break;
        }
        character.animator.SetBool("Dig", false);
        character.health.invulnerable = false;
        character.headbuttDamage = 10f;

        StartCoroutine(DigJumpAttack());
    }
    private IEnumerator DigJumpAttack()
    {
        character.velocity.y = jumpForce;
        character.animator.SetBool("Attack", true);
        float horizontalSpeed = transform.position.x - playerCharacter.transform.position.x >= 0 ? -1f : 1f;
        var loop = new WaitForFixedUpdate();

        while (true)
        {
            yield return loop;
            if (character.health.currentHealth <= 0)
            {
                StopAllCoroutines();
            }

            character.velocity.x = horizontalSpeed * digSpeed;
            character.FlipCharacter(character.velocity.x * -1f);

            if (character.velocity.y == 0)
                break;
        }
        character.velocity.x = 0f;
        character.animator.SetBool("Attack", false);
        brain.AbilityChange();
        IsDiggerAttack = false;
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
