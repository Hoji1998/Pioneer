using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class CharacterDash : CharacterAbility
{
    [Header("Dash Component")]
    [SerializeField] private KeyCode dashKey = KeyCode.C;
    [SerializeField] private float dashForce = 5f;
    [SerializeField] private float dashDuration = 1f;
    [SerializeField] private float dashBuffer = 0.5f;
    [SerializeField] private GameObject dashEffect;
    [SerializeField] private GameObject dashEffectPoint;
    [SerializeField] private GameObject photogene;
    private bool IsDash = false;
    private Coroutine coroutine;
    private void OnEnable()
    {
        Initialized();
    }
    public override void Initialized()
    {
        character = GetComponent<Character>();
        IsDash = false;
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
        if (Input.GetKeyDown(dashKey))
        {
            DashStart();
        }
    }

    private void DashStart()
    {
        coroutine = StartCoroutine(Dash());
    }
    private IEnumerator Dash()
    {
        IsDash = true;
        GameObject instanceParticle = Instantiate(dashEffect, dashEffectPoint.transform.position, dashEffectPoint.transform.rotation);
        instanceParticle.transform.localScale = new Vector2(transform.localScale.x * -1f, 1f);
        instanceParticle.transform.parent = character.model.transform;

        if (character.characterMoveState == Character.CharacterMoveState.Attack && character.characterWeaponAttack.currentWeapon != null)
        {
            PhotogeneOn();
        }

        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        float photogeneTime = 0f;
        float dashDirection = character.horizontalSpeed == 0 ? character.transform.localScale.x : character.horizontalSpeed;

        character.health.invulnerable = true;
        while (true)
        {
            yield return loop;
            AnimationStateChange(true);
            character.model.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);

            curTime += Time.deltaTime;

            character.velocity.x = dashDirection * dashForce;
            character.velocity.y = 0f;

            photogeneTime += Time.deltaTime;

            //if (photogeneTime >= dashDuration * 0.3f)
            //{
            //    photogeneTime = 0f;
            //    PhotogeneOn();
            //}

            if (curTime >= dashDuration || character.characterCondition == Character.CharacterCondition.Stunned)
            {
                break;
            }
        }
        character.model.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

        instanceParticle.transform.parent = null;
        character.velocity = Vector2.zero;

        AnimationStateChange(false);
        character.health.invulnerable = false;
        coroutine = StartCoroutine(DashBuffer());
    }
    private IEnumerator DashBuffer()
    {
        var waitTime = new WaitForFixedUpdate();
        float curTime = 0f;
        
        bool groundCheck = false;
        while (true)
        {
            yield return waitTime;
            curTime+= Time.deltaTime;
            if (character.IsGrounded || character.characterMoveState == Character.CharacterMoveState.Hanging
                || character.characterWeaponAttack.IsProceedFinishAttack)
            {
                groundCheck = true;
            }

            if (groundCheck && curTime >= dashBuffer)
            {
                break;
            }
        }
        IsDash = false;
    }
    public void PhotogeneOn()
    {
        GameObject instanceParticle_Photogene = Instantiate(photogene, character.transform.position, Quaternion.identity);
        instanceParticle_Photogene.transform.localScale = new Vector2(transform.localScale.x * -1f, 1f);
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

        if (character.characterMoveState == Character.CharacterMoveState.Attack && character.characterWeaponAttack.currentWeapon != null)
        {

            if (character.characterWeaponAttack.IsProceedFinishAttack)
            {
                return false;
            }
        }

        if (IsDash)
        {
            return false;
        }

        return true;
    }
    public override void AnimationStateChange(bool changeValue)
    {
        character.animator.SetBool(base.changeAnimationName, changeValue);

        if (changeValue)
        {
            character.characterMoveState = Character.CharacterMoveState.Dash;
        }
        else
        {
            character.InitializedAnimation();
        }
    }
}
