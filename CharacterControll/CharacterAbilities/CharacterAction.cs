using UnityEngine;
using DG.Tweening;
public class CharacterAction : CharacterAbility
{
    [Header("Action Component")]
    [SerializeField] private KeyCode actionKey = KeyCode.UpArrow;
    [SerializeField] private LayerMask ActionLayer;

    public bool IsAction = false;

    private ActionObject currentActionObject = null;
    private Collider2D actionObject;
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
        CheckActionObject();
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
        if (Input.GetKeyDown(actionKey))
        {
            IsAction = true;
            character.velocity = Vector2.zero;
            character.InitializedAnimation();
            currentActionObject.character = gameObject.GetComponent<Character>();
            currentActionObject.ActionStart();
        }
    }

    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned || character.characterType != Character.CharacterType.Player)
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

        if (currentActionObject == null || IsAction)
        {
            return false;
        }

        return true;
    }
    private void CheckActionObject()
    {
        actionObject = Physics2D.OverlapCircle(transform.position, 0.5f, ActionLayer);

        if (actionObject != null && !IsAction)
        {
            if (currentActionObject != null)
                return;

            currentActionObject = actionObject.GetComponent<ActionObject>();

            if (!currentActionObject.authorizedAction)
            {
                currentActionObject = null;
                return;
            }

            currentActionObject.ActionMarkerUpdate(true);
        }
        else
        {
            if (currentActionObject != null)
            {
                currentActionObject.ActionMarkerUpdate(false);
            }
            currentActionObject = null;
        }
    }
    private void AnimationStateChange(int changeValue)
    {
        character.animator.SetInteger(changeAnimationName, changeValue);
    }
}
