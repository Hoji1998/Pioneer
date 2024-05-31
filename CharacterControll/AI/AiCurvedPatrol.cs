using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class AiCurvedPatrol : AiAbility
{
    [Header("CurvedPatrol Component")]
    [SerializeField] private float patrolSpeed = 1.0f;
    [SerializeField] private GameObject curveObject;
    [Header("WayPoint")]
    [SerializeField] bool IsFoward = true;

    [HideInInspector] public GameObject CurveObject { get => curveObject; }
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        ChasingPlayer();
    }
    public override void ProcessAbility()
    {
        WallCheck();
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        switch (IsFoward)
        {
            case true:
                character.velocity.x = patrolSpeed;
                character.horizontalSpeed = patrolSpeed;
                CurveModel(1f);
                break;
            case false:
                character.velocity.x = patrolSpeed * -1f;
                character.horizontalSpeed = patrolSpeed * -1f;
                CurveModel(-1f);
                break;
        }
        AnimationStateChange(true);
    }
    private void WallCheck()
    {
        if (character.directionCollideState.leftWall)
        {
            IsFoward = true;
        }
        else if (character.directionCollideState.rightWall)
        {
            IsFoward = false;
        }
    }
    public void CurveModel(float value)
    {
        if (character.model.transform.localRotation != Quaternion.identity)
        {
            character.model.transform.rotation = Quaternion.identity;
        }

        if (Mathf.Abs(curveObject.transform.rotation.z - character.CurrentSlopeAngle * value) <= 0.1f)
        {
            return;
        }
        curveObject.transform.DORotate(new Vector3(0f, 0f, character.CurrentSlopeAngle * value), 0.2f);
    }
    private void ChasingPlayer()
    {
        if (LevelManager.Instance == null)
            return;

        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        if (character.transform.position.x - playerCharacter.transform.position.x < 0)
        {
            IsFoward = true;
        }
        else
        {
            IsFoward = false;
        }
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling)
        {
            return false;
        }

        if (brain.IsEngaged)
        {
            brain.AbilityChange();
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
