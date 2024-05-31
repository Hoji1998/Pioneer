using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CharacterJumping : CharacterAbility
{
    [Header("Jump Component")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Z;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private GameObject jumpFeedback;
    [SerializeField] private GameObject jumpEffectPoint;

    [Header("Long Jump")]
    public bool authorizedLongJump = false;
    public float lowGravity = -20f;
    public float highGravity = -30f;

    [Header("Multiple Jump")]
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private int currentJumpCount = 0;

    [Header("LandingRigidity Component")]
    [SerializeField] private float landingRigidityTime = 1.0f;
    public bool IsLongJump { set; get; } = false;
    private float currentLongJumpBuffer = 0f;
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
        InitializedJumpCount();
        JumpAnimationCheck();
        LandingRigidityCheck();
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
        if (Input.GetKeyDown(jumpKey) && character.verticalSpeed < 0 && character.IsOneWayPlatform)
        {
            character.RaycastVerticalPauseEvent();
            return;
        }

        if (Input.GetKeyDown(jumpKey) && character.characterMoveState != Character.CharacterMoveState.Attack)
        {
            bool IsJump = character.JumpTo(jumpForce, ref currentJumpCount);
            Instantiate(jumpFeedback, jumpEffectPoint.transform).transform.parent = null;
            if (IsJump)
            {
                character.characterMoveState = Character.CharacterMoveState.Jumping;
                //jump event
            }
        }
        if (Input.GetKey(jumpKey))
        {
            IsLongJump = true;
            
            currentLongJumpBuffer += Time.deltaTime;
        }
        else if (Input.GetKeyUp(jumpKey))
        {
            IsLongJump = false;
            currentLongJumpBuffer = 0f;
            character.velocity.y = 0f;
            //if (currentLongJumpBuffer < longJumpBuffer)
            //{
                
            //}
        }
    }
    private float landingTime = 1f;
    private float currentLandingTime = 0f;
    private bool IsLandingRigidity = false;
    private void LandingRigidityCheck()
    {
        if (character.characterMoveState == Character.CharacterMoveState.Falling)
        {
            currentLandingTime += Time.deltaTime;
        }
        else
        {
            currentLandingTime = 0f;
        }

        if (currentLandingTime >= landingTime)
        {
            IsLandingRigidity = true;
        }

        AuthorizedLandingRigidity();
    }
    private void AuthorizedLandingRigidity()
    {
        if (!IsLandingRigidity)
            return;

        if (!character.IsGrounded)
            return;

        IsLandingRigidity = false;
        StartCoroutine(LandingRigidity());
    }
    private IEnumerator LandingRigidity()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        character.animator.SetBool("LandingRigidity", true);
        character.velocity = Vector2.zero;
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);

        yield return new WaitForSeconds(landingRigidityTime);
        character.animator.SetBool("LandingRigidity", false);
        character.characterCondition = Character.CharacterCondition.Normal;
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

        if (character.characterMoveState == Character.CharacterMoveState.Attack && 
            (character.IsGrounded ||
            character.characterWeaponAttack.IsProceedFinishAttack))
        {
            return false;
        }

        return true;
    }
    private void JumpAnimationCheck()
    {
        if (character.characterMoveState == Character.CharacterMoveState.Attack)
            return;

        if (character.velocity.y > 0)
        {
            AnimationStateChange(1);
            //character.characterMoveState = Character.CharacterMoveState.Jumping;
        }
        else if (character.velocity.y < 0)
        {
            AnimationStateChange(-1);
            //character.characterMoveState = Character.CharacterMoveState.Falling;
        }
        else if (character.velocity.y == 0)
        {
            AnimationStateChange(0);
        }
    }
    public void JumpCancle()
    {
        IsLongJump = false;
        currentLongJumpBuffer = 0f;
        character.velocity.y = 0f;
    }
    public void LongJumpGravityCheck(Character character)
    {
        if (!authorizedLongJump)
            return;

        if (character.characterCondition == Character.CharacterCondition.Stunned)
            return;

        if (IsLongJump && character.velocity.y > 0)
        {
            character.gravityScale = lowGravity;
        }
        else
        {
            character.gravityScale = highGravity;
        }
    }
    private void InitializedJumpCount()
    {
        if ((character.IsGrounded && character.velocity.y <= 0)
            || character.characterMoveState == Character.CharacterMoveState.Hanging)
        {
            character.velocity.y = 0f;
            currentJumpCount = maxJumpCount;
            currentLongJumpBuffer = 0f;
            authorizedLongJump= true;
        }
    }
    private void AnimationStateChange(int changeValue)
    {
        character.animator.SetInteger(changeAnimationName, changeValue);
    }
}
