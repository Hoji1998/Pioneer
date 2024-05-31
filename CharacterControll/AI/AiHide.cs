using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiHide : AiAbility
{
    [Header("Hide Component")]
    [SerializeField] private float findXrange = 1f;
    private bool IsAwake = false;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
    }
    private void OnEnable()
    {
        IsAwake = false;
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
        if (CheckAttackRange())
        {
            AnimationStateChange(false);
            IsAwake = true;
            return;
        }

        AnimationStateChange(true);
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return false;
        }

        if (IsAwake)
        {
            brain.AbilityChange();
            return false;
        }
        return true;
    }
    private bool CheckAttackRange()
    {
        if (playerCharacter == null)
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();

        if (playerCharacter.transform.position.y > character.transform.position.y)
            return false;

        if (Mathf.Abs(playerCharacter.transform.position.x - character.transform.position.x) > findXrange)
            return false;

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
