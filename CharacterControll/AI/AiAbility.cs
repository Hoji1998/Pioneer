using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AiAbility : MonoBehaviour
{
    [Header("Ai State")]
    [HideInInspector] public AiBrain brain;
    [Header("Animation")]
    public string changeAnimationName;

    [HideInInspector] public Character character;
    [HideInInspector] public Character playerCharacter;
    public virtual void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    public virtual void InitializedAbility()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }

        character.velocity = Vector2.zero;
        character.horizontalSpeed = 0;
    }

    public virtual void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public virtual bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return false;
        }
        return true;
    }
    public virtual void AbilityStart()
    {
        
    }

    public virtual void AbilityStop() 
    {
        
    }
    public virtual void AnimationStateChange(bool changeValue)
    {
        character.animator.SetBool(changeAnimationName, changeValue);

        if (changeValue)
        {
            brain.currentAiState = AiBrain.AiState.Proceeding;
        }
    }
}
