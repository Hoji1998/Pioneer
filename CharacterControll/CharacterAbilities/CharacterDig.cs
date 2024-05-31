using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDig : CharacterAbility
{
    [Header("Dig Component")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float digStopSpeed = 10f;

    [Header("Dig Particle")]
    [SerializeField] private GameObject digDashParticle;
    [SerializeField] private GameObject digDiveParticle;
    [SerializeField] private TrailRenderer digTrail;

    [HideInInspector] public bool IsDigStop = false;
    private Coroutine coroutine;
    public override void Start()
    {
        Initialized();
    }

    public override void Initialized()
    {
        character = GetComponent<Character>();
        digTrail.transform.parent = null;
    }

    public override void Update()
    {
        ProcessAbility();
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
        {
            if (digTrail.transform.parent != null)
            {
                digTrail.transform.parent = null;
            }
            return;
        }

        Digging();
    }
    private void Digging()
    {
        if (digTrail.transform.parent != transform)
        {
            digTrail.transform.parent = transform;
            digTrail.transform.localPosition = new Vector3(0, -0.11f, 0f);
            digTrail.Clear();
            character.velocity *= 0.1f;
        }
        character.velocity = Vector2.Lerp(character.velocity, new Vector2(character.horizontalSpeed, character.verticalSpeed) * moveSpeed, Time.deltaTime);
        //character.MoveToHorizontal(moveSpeed);
        //character.MoveToVertical(moveSpeed);
        AnimationStateChange(true);
        character.FlipCharacter(character.velocity.x);
        if (Mathf.Abs(character.velocity.x) < 0.05f && Mathf.Abs(character.velocity.y) < 0.05f)
            return;

        ModelRotate();
    }
    private void ModelRotate()
    {
        Vector2 drillDirection = character.velocity;
        float drillAngle = Mathf.Atan2(drillDirection.y, drillDirection.x) * Mathf.Rad2Deg;
        Quaternion angleAxis_model = Quaternion.identity;

        switch (character.transform.localScale.x)
        {
            case -1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle - 90f, Vector3.forward);
                break;
            case 1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle - 90f, Vector3.forward);
                break;
        }


        Quaternion modelRotation = Quaternion.Slerp(character.model.transform.rotation, angleAxis_model, 1f);
        character.model.transform.rotation = modelRotation;
    }
    public override void StopAbility()
    {
        character.characterJumping.authorizedLongJump = false;
        IsDigStop = true;

        if (!gameObject.activeSelf)
            return;

        coroutine = StartCoroutine(DigStop());
    }
    private IEnumerator DigStop()
    {
        float curTime = 0f;
        digDashParticle.SetActive(true);

        while (true)
        {
            yield return new WaitForFixedUpdate();
            curTime += Time.deltaTime;
            if (curTime >= 0.3f)
                break;

            float force = digStopSpeed - (curTime * digStopSpeed * 2f);

            if (force < 0)
                force = 0;

            character.velocity = character.velocity.normalized * force;
            AnimationStateChange(true);
        }
        AnimationStateChange(false);
        character.InitializedAnimation();

        IsDigStop = false;
        digDashParticle.SetActive(false);
        character.model.transform.rotation = Quaternion.identity;
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned
            || !character.IsSand
            || IsDigStop)
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
            character.characterMoveState = Character.CharacterMoveState.Dig;
        }
    }
}
