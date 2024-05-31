using UnityEngine;

public class CharacterMoving : CharacterAbility
{
    [Header("Moving Component")]
    [SerializeField] private float moveSpeed = 2f;

    [HideInInspector] public float MoveSpeed { get => moveSpeed; }
    public override void Start()
    {
        Initialized();
    }

    public override void Initialized()
    {
        character = GetComponent<Character>();
    }

    public override void Update()
    {
        ProcessAbility();
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        StartAbility();
    }
    public override void StartAbility()
    {
        if (character.horizontalSpeed == 0)
        {
            StopAbility();
            return;
        }
        character.MoveToHorizontal(moveSpeed);

        if (character.animator.GetInteger("y") == 0)
        {
            AnimationStateChange(true);
        }
    }

    public override void StopAbility()
    {
        AnimationStateChange(false);
        character.MoveToHorizontal(0f);
    }

    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return false;
        }

        foreach (Character.CharacterMoveState blockState in base.BlockState)
        {
            if (character.characterMoveState == blockState)
            {
                return false;
            }
        }

        return true;
    }
    public override void AnimationStateChange(bool changeValue)
    {
        character.animator.SetBool(base.changeAnimationName, changeValue);

        if (changeValue)
        {
            character.characterMoveState = Character.CharacterMoveState.Walking;
        }
    }
}
