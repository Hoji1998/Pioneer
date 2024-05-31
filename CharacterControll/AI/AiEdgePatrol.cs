using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AiEdgePatrol : AiAbility
{
    [Header("WallEdgePatrol Component")]
    [SerializeField] private float patrolSpeed = 1.0f;
    [SerializeField] private bool IsFoward = true;
    [SerializeField] private GameObject rotatePoint;

    private Vector2 moveDirection;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
        AbilityStart();
    }
    private void OnEnable()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }

        character.velocity = Vector2.zero;
        character.transform.rotation = Quaternion.identity;
        character.model.transform.rotation = Quaternion.identity;
        rotateValue = 0f;
        IsCurve = false;
        IsRotate = false;
    }
    public override void InitializedAbility()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        moveDirection = transform.right;
        if (IsFoward)
        {
            character.velocity = (moveDirection * patrolSpeed) + (Vector2)transform.up * -1f * patrolSpeed;
        }
        else
        {
            character.velocity = (moveDirection * -patrolSpeed) + (Vector2)transform.up * -1f * patrolSpeed;
        }

        CurveCheck();
        AnimationStateChange(true);
    }
    private bool IsRotate = false;
    private bool IsCurve = false;
    private float rotateValue = 0f;
    private void CurveCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(rotatePoint.transform.position, transform.up * -1f, 0.5f, character.collisionLayer);
        Debug.DrawRay(rotatePoint.transform.position, transform.up * -1f, Color.magenta);
        if (hit.collider == null && !IsCurve)
        {
            IsRotate = true;
            IsCurve = true;
            character.velocity = Vector2.zero;

            rotateValue -= 90f;
            if (rotateValue <= -360f)
            {
                rotateValue = 0f;
            }
            character.transform.DORotate(new Vector3(0f, 0f, rotateValue), 0.1f).OnComplete(InitializeRotateState);
        }

        if (hit)
        {
            IsCurve = false;
        }
        
    }
    private void InitializeRotateState()
    {
        IsRotate = false;
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            !gameObject.activeSelf)
        {
            return false;
        }

        if (IsRotate)
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
