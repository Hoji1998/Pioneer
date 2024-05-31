using UnityEngine;

public abstract class CharacterAbility : MonoBehaviour
{
    [Header("Initialized")]
    public bool authorizedAbility = true;
    public Character.CharacterMoveState[] BlockState;
    public string changeAnimationName = "";

    [HideInInspector] public Character character;
    virtual public void Start()
    {
        Initialized();
    }

    virtual public void Initialized()
    {
        character = GetComponent<Character>();
    }

    virtual public void Update()
    {
        ProcessAbility();
    }
    virtual public void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        StartAbility();
    }
    virtual public void StartAbility()
    {
        Debug.Log("AbilityStart");
    }
    virtual public void StopAbility()
    {
        Debug.Log("AbilityStop");
    }
    virtual public bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned
            || !authorizedAbility)
        {
            return false;
        }

        foreach (Character.CharacterMoveState blockState in BlockState)
        {
            if (character.characterMoveState == blockState)
            {
                return false;
            }
        }

        return true;
    }
    virtual public void AnimationStateChange(bool changeValue)
    {
        character.animator.SetBool(changeAnimationName, changeValue);

        if (changeValue)
        {
            character.characterMoveState = Character.CharacterMoveState.Idle;
        }
    }
}
