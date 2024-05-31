using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CharacterWeaponAttack : CharacterAbility
{
    [Header("Weapon")]
    [SerializeField] private GameObject meleeWeaponObject;
    [SerializeField] private GameObject jumpWeaponObject;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private KeyCode attackKey = KeyCode.Space;
    [SerializeField] private float damage = 1f;
    [SerializeField] private float attackBuffer = 0.5f;

    [Header("Weapon MovingAttack")]
    [SerializeField] private Vector2 preparingMovingForce = Vector2.zero;
    [SerializeField] private Vector2 underAttackMovingForce = Vector2.zero;
    [SerializeField] private Vector2 attackDoneMovingForce = Vector2.zero;

    [Header("ComboAttack")]
    [SerializeField] private int maxComboAttackCount = 1;
    [SerializeField] private float comboAttackBuffer = 0.5f;

    [Header("FinishAttackEffect")]
    [SerializeField] private LineRenderer finishLine;
    [SerializeField] private GameObject drillDashParticle;
    [SerializeField] private GameObject lineEffect;
    [SerializeField] private GameObject finishParticle;
    [SerializeField] private GameObject finishParticlePosition;
    [SerializeField] private LineRenderer throwDrillLine;
    [SerializeField] private GameObject throwDrill;
    [SerializeField] private GameObject drillChargeEffect;
    [SerializeField] private GameObject finishDoneParticle;
    [SerializeField] private GameObject finishDoneParticlePosition;
    [SerializeField] private GameObject photogene;

    [Header("FinishAttack Component")]
    [SerializeField] private float finishAttackDuration = 0.5f;
    [SerializeField] private float throwDrillDelay = 0.2f;
    [SerializeField] private float InertiaTime = 0.2f;
    [SerializeField] private KeyCode finishAttackKey = KeyCode.LeftControl;
    [SerializeField] private LayerMask finishAttackLayer;

    [HideInInspector] public bool IsProceedFinishAttack = false;
    [HideInInspector] public Weapon currentWeapon;
    [HideInInspector] public List<FinishAttackPoint> finishAttackObjects;
    [HideInInspector] public FinishAttackPoint CurrentFinishAttackObject { get => currentFinishAttackObject; }
    [HideInInspector] public int CurrentComboAttackCount { get => currentComboAttackCount; }

    private FinishAttackPoint currentFinishAttackObject;
    private FinishAttackPoint lastFinishAttackObject;
    private GameObject characterWeapon;
    private Weapon meleeWeapon;
    private Weapon jumpWeapon;
    private Coroutine coroutine;
    private int currentComboAttackCount = 0;
    private float currentHorizontalSpeed = 0;
    private float currentAttackBuffer = 0f;
    private bool IsAdvanceInput = false;
    private bool IsJumpingAttack = false;
    RaycastHit2D wallHit;
    private void OnEnable()
    {
        Initialized();
    }
    public override void Initialized()
    {
        character = GetComponent<Character>();

        if (meleeWeapon == null)
        {
            characterWeapon = Instantiate(meleeWeaponObject);
            characterWeapon.transform.parent = character.gameObject.transform;
            characterWeapon.transform.localPosition = weaponTransform.localPosition;
            characterWeapon.transform.rotation = weaponTransform.rotation;
            characterWeapon.transform.localScale = weaponTransform.localScale;
            meleeWeapon = characterWeapon.GetComponent<Weapon>();
            meleeWeapon.character = character;
            meleeWeapon.weaponDamage = damage;

            if (meleeWeapon.Model.GetComponent<SpriteRenderer>() != null)
            {
                meleeWeapon.Model.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
            }
        }

        if (jumpWeapon == null)
        {
            characterWeapon = Instantiate(jumpWeaponObject);
            characterWeapon.transform.parent = character.gameObject.transform;
            characterWeapon.transform.localPosition = weaponTransform.localPosition;
            characterWeapon.transform.rotation = weaponTransform.rotation;
            characterWeapon.transform.localScale = weaponTransform.localScale;
            jumpWeapon = characterWeapon.GetComponent<Weapon>();
            jumpWeapon.character = character;
            jumpWeapon.weaponDamage = damage;

            if (jumpWeapon.Model.GetComponent<SpriteRenderer>() != null)
            {
                jumpWeapon.Model.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
            }
        }
        

        currentWeapon = meleeWeapon;
        currentAttackBuffer = 0f;

        finishLine.gameObject.SetActive(true);
        lineEffect.transform.parent = null;
        InitializedFinishLine();
    }
    public override void Update()
    {
        CheckHangingAnimation();
        InitializedComboAttackCount();
        FinishAttackObjectCheck();
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
        if (AuthorizedAdvanceInput())
            return;

        if ((Input.GetKeyDown(attackKey) || IsAdvanceInput) && currentComboAttackCount < maxComboAttackCount)
        {
            SelectWeapon();
            character.FlipCharacter(character.horizontalSpeed);
            currentHorizontalSpeed = character.horizontalSpeed;
            currentWeapon.currentCombo = currentComboAttackCount;
            currentComboAttackCount++;
            currentAttackBuffer = 0f;
            IsAdvanceInput = false;

            ComboAttackBufferUpdate();
            currentWeapon.OperateAttack();

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = IsJumpingAttack ? StartCoroutine(AttackMovement_Jumping()) : StartCoroutine(AttackMovement());

            currentWeapon.characterWeaponAttack = this;
            
        }
        else if (Input.GetKeyDown(finishAttackKey) 
            && character.characterMoveState != Character.CharacterMoveState.Attack
            && currentFinishAttackObject != null)
        {
            FinishAttack();
        }
    }
    private bool AuthorizedAdvanceInput()
    {
        if (currentWeapon.currentWeaponState != Weapon.WeaponState.Ready
            && currentAttackBuffer < attackBuffer)
        {
            currentAttackBuffer += Time.deltaTime;
            if (Input.GetKeyDown(attackKey) && currentComboAttackCount < maxComboAttackCount)
            {
                IsAdvanceInput = true;
            }
            return true;
        }

        if (character.characterMoveState == Character.CharacterMoveState.Dash)
        {
            if (Input.GetKeyDown(attackKey) && currentComboAttackCount < maxComboAttackCount)
            {
                IsAdvanceInput = true;
            }
            return true;
        }

        return false;
    }
    private void InitializedComboAttackCount()
    {
        if (currentWeapon.currentWeaponState == Weapon.WeaponState.Ready && 
            (character.IsGrounded || 
            character.characterMoveState == Character.CharacterMoveState.Hanging ||
            character.characterMoveState == Character.CharacterMoveState.Dash ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling))
        {
            currentComboAttackCount = 0;
            currentWeapon.currentCombo = 0;
        }
        character.animator.SetInteger("comboAttack", currentComboAttackCount);
    }
    private void ComboAttackBufferUpdate()
    {
        if (currentComboAttackCount == maxComboAttackCount)
        {
            currentWeapon.comboAttackBuffer = comboAttackBuffer;
        }
        else
        {
            currentWeapon.comboAttackBuffer = comboAttackBuffer;
        }
    }
    private void SelectWeapon()
    {
        if (currentComboAttackCount != 0)
            return;

        if (character.characterMoveState == Character.CharacterMoveState.Jumping
            || character.characterMoveState == Character.CharacterMoveState.Falling)
        {
            currentWeapon = jumpWeapon;
            IsJumpingAttack = true;
            maxComboAttackCount = 2;
            return;
        }

        maxComboAttackCount = 3;
        currentWeapon = meleeWeapon;
        IsJumpingAttack = false;
    }
    private void CheckHangingAnimation()
    {
        //Player Hanging Check
        if (character.characterMoveState == Character.CharacterMoveState.Hanging)
        {
            character.animator.SetBool("Hanging", true);
        }
        else
        {
            character.animator.SetBool("Hanging", false);
        }
        //
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned
            || !authorizedAbility
            || currentWeapon.currentWeaponState == Weapon.WeaponState.PreparingAttack
            || IsProceedFinishAttack)
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

    #region AttackMovement
    private IEnumerator AttackMovement()
    {
        character.characterMoveState = Character.CharacterMoveState.Attack;
        while (true)
        {
            yield return null;
            if (character.characterCondition == Character.CharacterCondition.Stunned
                        || character.characterMoveState == Character.CharacterMoveState.Dash
                        || character.characterAction.IsAction)
            {
                currentWeapon.InitializedAttackState();
                break;
            }

            if (Input.GetKeyDown(finishAttackKey)
            && currentFinishAttackObject != null)
            {
                currentWeapon.InitializedAttackState();
                FinishAttack();
                break;
            }

            switch (currentWeapon.currentWeaponState)
            {
                case Weapon.WeaponState.PreparingAttack:
                    character.animator.SetBool("PreparingAttack", true);
                    character.animator.SetBool("UnderAttack", false);
                    character.animator.SetBool("AttackDone", false);
                    character.velocity.x = preparingMovingForce.x;
                    break;
                case Weapon.WeaponState.UnderAttack:
                    character.animator.SetBool("UnderAttack", true);
                    character.animator.SetBool("PreparingAttack", false);
                    character.animator.SetBool("AttackDone", false);
                    character.velocity.x = underAttackMovingForce.x;
                    break;
                case Weapon.WeaponState.AttackDone:
                    character.animator.SetBool("PreparingAttack", false);
                    character.animator.SetBool("UnderAttack", false);
                    character.animator.SetBool("AttackDone", true);
                    character.velocity.x = attackDoneMovingForce.x;
                    break;
                case Weapon.WeaponState.Ready:
                    character.animator.SetBool("PreparingAttack", false);
                    character.animator.SetBool("UnderAttack", false);
                    character.animator.SetBool("AttackDone", false);
                    StopCoroutine(coroutine);
                    break;
            }

            if (currentHorizontalSpeed == 0 || currentComboAttackCount == 1)
                character.velocity.x = 0;

            character.velocity.x *= character.transform.localScale.x;

            //moving platform attack
            character.GravityScaleUpdate();
        }
    }
    private IEnumerator AttackMovement_Jumping()
    {
        //character.characterJumping.JumpCancle();
        character.characterMoveState = Character.CharacterMoveState.Attack;
        character.velocity *= 0.8f;
        while (true)
        {
            yield return null;
            if (character.characterCondition == Character.CharacterCondition.Stunned
                        || character.characterMoveState == Character.CharacterMoveState.Dash
                        || character.characterAction.IsAction)
            {
                currentWeapon.InitializedAttackState();
                break;
            }

            if (Input.GetKeyDown(finishAttackKey)
            && currentFinishAttackObject != null)
            {
                currentWeapon.InitializedAttackState();
                FinishAttack();
                break;
            }

            switch (currentWeapon.currentWeaponState)
            {
                case Weapon.WeaponState.PreparingAttack:
                    character.animator.SetBool("PreparingAttack_Jump", true);
                    character.animator.SetBool("UnderAttack_Jump", false);
                    character.animator.SetBool("AttackDone_Jump", false);
                    break;
                case Weapon.WeaponState.UnderAttack:
                    character.animator.SetBool("UnderAttack_Jump", true);
                    character.animator.SetBool("PreparingAttack_Jump", false);
                    character.animator.SetBool("AttackDone_Jump", false);
                    break;
                case Weapon.WeaponState.AttackDone:
                    character.animator.SetBool("PreparingAttack_Jump", false);
                    character.animator.SetBool("UnderAttack_Jump", false);
                    character.animator.SetBool("AttackDone_Jump", true);
                    break;
                case Weapon.WeaponState.Ready:
                    character.animator.SetBool("PreparingAttack_Jump", false);
                    character.animator.SetBool("UnderAttack_Jump", false);
                    character.animator.SetBool("AttackDone_Jump", false);
                    StopCoroutine(coroutine);
                    break;
            }
            if (character.IsGrounded)
            {
                character.velocity.x = 0f;
            }
            else 
            {
                character.MoveToHorizontal(character.characterMoving.MoveSpeed);
            }
            character.GravityScaleUpdate();
            //character.velocity.y += character.gravityScale * Time.deltaTime;
        }
    }
    #endregion

    #region ThrowDrill
    private IEnumerator ThrowDrill()
    {
        Vector2 drillPos = transform.position;
        throwDrill.SetActive(true);
        drillChargeEffect.SetActive(true);
        float curTime = 0f;
        bool IsDrillRotate = true;
        IsProceedFinishAttack = true;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            // -- drill move Authorized check--
            if (!AuthorizedDrillMove())
            {
                break;
            }

            // -- drill move --
            ThrowDrillMovement(ref drillPos);

            // -- drill move complete --
            if (Vector2.Distance(drillPos, currentFinishAttackObject.transform.position) <= 0.1f)
            {
                IsDrillRotate = false;
                curTime += Time.deltaTime;
                if (curTime >= throwDrillDelay)
                {
                    character.velocity = Vector2.zero;
                    coroutine = StartCoroutine(FinishAttackStart());
                    break;
                }
            }

            if (IsDrillRotate)
            {
                DrillRotate();
            }
        }
    }
    private bool AuthorizedDrillMove()
    {
        if (ThrowDrillWallCheck() 
            || character.characterCondition == Character.CharacterCondition.Stunned
            || character.characterMoveState == Character.CharacterMoveState.Dash)
        {
            AnimationStateChange(false);
            InitializedFinishLine();
            return false;
        }
        return true;
    }
    private void ThrowDrillMovement(ref Vector2 drillPos)
    {
        drillPos = Vector2.MoveTowards(drillPos, currentFinishAttackObject.transform.position, Time.deltaTime * finishAttackDuration * 0.5f);

        throwDrillLine.SetPosition(0, transform.position);
        throwDrillLine.SetPosition(1, drillPos);
        throwDrill.transform.position = drillPos;
        character.velocity *= 0.7f;

        if (!throwDrillLine.gameObject.activeSelf)
        {
            throwDrillLine.gameObject.SetActive(true);
        }
    }
    private bool ThrowDrillWallCheck()
    {
        wallHit = Physics2D.Raycast(transform.position, currentFinishAttackObject.transform.position - transform.position,
           Vector2.Distance(transform.position, currentFinishAttackObject.transform.position), character.collisionLayer - character.onewayPlatformLayer);

        if (wallHit)
        {
            return true;
        }

        return false;
    }
    private void DrillRotate()
    {
        Vector2 drillDirection = new Vector2(
                throwDrill.transform.position.x - currentFinishAttackObject.transform.position.x,
                throwDrill.transform.position.y - currentFinishAttackObject.transform.position.y);
        float drillAngle = Mathf.Atan2(drillDirection.y, drillDirection.x) * Mathf.Rad2Deg;
        Quaternion angleAxis_drill = Quaternion.AngleAxis(drillAngle - 90f, Vector3.forward);
        Quaternion angleAxis_model = Quaternion.identity;

        switch (character.transform.localScale.x)
        {
            case -1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle, Vector3.forward);
                break;
            case 1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle + 180f, Vector3.forward);
                break;
        }
        
        Quaternion drillRotation = Quaternion.Slerp(throwDrill.transform.rotation, angleAxis_drill, 1f);
        Quaternion modelRotation = Quaternion.Slerp(character.model.transform.rotation, angleAxis_model, 1f);

        throwDrill.transform.rotation = drillRotation;

        character.model.transform.rotation = modelRotation;
        character.FlipCharacter((throwDrill.transform.position.x - currentFinishAttackObject.transform.position.x) * -1f);
        
    }
    #endregion

    #region FinishAttack
    private void FinishAttackObjectCheck()
    {
        if (character.characterMoveState == Character.CharacterMoveState.Attack)
        {
            return;
        }

        if (finishAttackObjects.Count == 0)
        {
            currentFinishAttackObject = null;
            return;
        }

        float minimumDistance = 0f;

        foreach (FinishAttackPoint finishAttackPoint in finishAttackObjects)
        {
            if (lastFinishAttackObject == finishAttackPoint && finishAttackObjects.Count > 1)
            {
                continue;
            }

            if (minimumDistance == 0f)
            {
                minimumDistance = finishAttackPoint.currentFinishAttackDistance;
                currentFinishAttackObject = finishAttackPoint;
            }
            else if (minimumDistance > finishAttackPoint.currentFinishAttackDistance || finishAttackPoint.IsEnemy)
            {
                minimumDistance = finishAttackPoint.currentFinishAttackDistance;
                currentFinishAttackObject = finishAttackPoint;
            }
        }
    }
    public void InitializedFinishLine()
    {
        finishLine.startColor = new Color(0f, 0f, 0f, 0f);
        finishLine.endColor = new Color(0f, 0f, 0f, 0f);
        throwDrill.SetActive(false);
        throwDrillLine.gameObject.SetActive(false);
        drillChargeEffect.SetActive(false);
        IsProceedFinishAttack = false;
        character.model.transform.rotation = Quaternion.identity;
    }
    private void FinishAttack()
    {
        AnimationStateChange(true);
        character.animator.SetBool("FinishAttack", true);

        Vector2 moveDir = currentFinishAttackObject.transform.position - transform.position;

        if (moveDir.x < 0)
            character.FlipCharacter(-1f);
        else
            character.FlipCharacter(1f);

        coroutine = StartCoroutine(ThrowDrill());
    }
    Vector3 drillVelocity;
    private IEnumerator FinishAttackStart()
    {
        character.health.invulnerable = true;
        GameObject instanceParticle = Instantiate(finishParticle, finishParticlePosition.transform.position, character.model.transform.rotation);

        GameObject instanceParticle_wind = Instantiate(finishDoneParticle, finishDoneParticlePosition.transform.position, character.model.transform.rotation);
        instanceParticle_wind.transform.localScale = new Vector2(transform.localScale.x, 1f);
        instanceParticle_wind.transform.parent = character.model.transform;

        drillDashParticle.SetActive(true);

        lineEffect.transform.rotation = character.model.transform.rotation;
        lineEffect.SetActive(false);

        drillChargeEffect.SetActive(false);

        drillVelocity = (currentFinishAttackObject.transform.position - transform.position).normalized;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            transform.position = Vector2.MoveTowards(transform.position, currentFinishAttackObject.transform.position, Time.deltaTime * finishAttackDuration);

            throwDrillLine.SetPosition(0, transform.position);
            throwDrillLine.SetPosition(1, currentFinishAttackObject.transform.position);
            throwDrill.transform.position = currentFinishAttackObject.transform.position;
            PhotogeneOn();
            if (Vector2.Distance(transform.position, currentFinishAttackObject.transform.position) <= 0.1f)
                break;
        }
        instanceParticle_wind.transform.parent = null;
        drillDashParticle.SetActive(false);
        coroutine = StartCoroutine(FinishAttackComplete());
    }
    private IEnumerator FinishAttackComplete()
    {
        FinishAttackDone();
        yield return new WaitForSeconds(InertiaTime);
        character.health.invulnerable = false;
    }
    private void FinishAttackDone()
    {
        switch (currentFinishAttackObject.IsEnemy)
        {
            case true:
                lineEffect.transform.position = transform.position;
                lineEffect.SetActive(true);
                coroutine = StartCoroutine(FinishAttackInertia());
                break;
            case false:
                AnimationStateChange(false);
                lastFinishAttackObject = currentFinishAttackObject;
                break;
        }

        currentFinishAttackObject.GetComponent<FinishAttackPoint>().FinishObjectFeedback();
        InitializedFinishLine();
    }
    private IEnumerator FinishAttackInertia()
    {
        var loop = new WaitForFixedUpdate();
        float curTime = 0;
        while (true)
        {
            yield return loop;
            curTime += Time.deltaTime;
            character.velocity = drillVelocity * 10f;
            if (character.velocity.y > 5f)
            {
                character.velocity.y = 7f;
            }

            if (curTime >= InertiaTime
                || character.characterMoveState == Character.CharacterMoveState.Dash)
            {
                break;
            }
        }
        AnimationStateChange(false);
    }
    #endregion
    public void PhotogeneOn()
    {
        GameObject instanceParticle_Photogene = Instantiate(photogene, character.transform.position, character.model.transform.rotation);
        instanceParticle_Photogene.transform.localScale = new Vector2(transform.localScale.x * -1f, 1f);
    }
    public override void AnimationStateChange(bool changeValue)
    {
        
        if (changeValue)
        {
            character.characterMoveState = Character.CharacterMoveState.Attack;
        }
        else
        {
            character.animator.SetBool("PreparingAttack", false);
            character.animator.SetBool("UnderAttack", false);
            character.animator.SetBool("AttackDone", false);
            character.animator.SetBool("FinishAttack", false);
            character.animator.SetBool("PreparingAttack_Jump", false);
            character.animator.SetBool("UnderAttack_Jump", false);
            character.animator.SetBool("AttackDone_Jump", false);
            character.InitializedAnimation();
        }
    }
}
