using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiFly : AiAbility
{
    [Header("Fly Component")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private Vector3 distancingPoint;

    private Vector2 direction= Vector2.zero;
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
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        CalculateDirection();

        character.velocity += direction.normalized * speed * Time.deltaTime;
        LimitSpeedCheck();
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
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return false;
        }
        return true;
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
