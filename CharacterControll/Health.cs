using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Health : FinishAttackPoint
{
    [Header("Health Component")]
    public float currentHealth = 100f;
    public float maxHealth = 100f;
    public bool invulnerable = false;
    public bool invulnerableInfinity = false;
    public float invulnerableBuffer = 0.2f;
    public float initialHealth = 100f;

    [Header("Hit Effect")]
    [SerializeField] private string changeAnimationName = "hit";
    [SerializeField] private GameObject hitFeedback;
    [SerializeField] private Vector2 knockbackForce = Vector2.zero;
    [SerializeField] private float hitGravityScale = -15f;

    [Header("Death")]
    public Animator guardBreakPointAnim;
    public bool IsGuardeBreakableHealth = true;
    [SerializeField] private GameObject deathFeedBack;
    [SerializeField] private AiAbility destroyAiAbility;
    [SerializeField] private float guardBreakTime = 3f;
    [SerializeField] private float finishAttackWaitTime = 0.3f;

    private float initInvulnerableBuffer = 0.2f;
    private Character character;
    private Coroutine coroutine;
    private SpriteRenderer sprite;
    private GUIManager guiManager;
    private int IsForwardKnockback = 1;
    public override void Start()
    {
        base.Start();
        character = GetComponent<Character>();
        sprite = character.model.GetComponent<SpriteRenderer>();
        guiManager = GUIManager.Instance;
    }
    public override void FinishObjectFeedback()
    {
        if (destroyAiAbility != null)
        {
            destroyAiAbility.AbilityStart();
            return;
        }
        Destroy();
    }
    public void InitializedHealth()
    {
        if (authorizedAbility)
        {
            guardBreakPointAnim.SetBool("GuardBreak", false);
            AnimationStateChange(false);
        }
        
        invulnerable = false;
        authorizedAbility = false;
        currentHealth = initialHealth;

        if (guiManager == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
        {
            invulnerableBuffer = initInvulnerableBuffer;
            return;
        }
        guiManager.UpdateHealth();
    }
    private void OnEnable()
    {
        InitializedHealth();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!AuthorizedDamage())
            return;

        Weapon weapon = collision.gameObject.GetComponent<Weapon>();
        weapon = weapon == null ? collision.gameObject.GetComponentInParent<Weapon>() : null;
        Character other = collision.gameObject.GetComponent<Character>();

        IsForwardKnockback = transform.position.x - collision.gameObject.transform.position.x < 0 ? 1 : -1;
        WeaponCollideCheck(weapon);
        HeadbuttCheck(other);
    }
    private void WeaponCollideCheck(Weapon weapon)
    {
        if (weapon == null)
            return;

        if (character.GetComponent<CharacterWeaponAttack>() == null)
        {
            DamageCalculate(weapon.weaponDamage);
        }
        else if (weapon != character.GetComponent<CharacterWeaponAttack>().currentWeapon) //내 무기에 안맞기
        {
            DamageCalculate(weapon.weaponDamage);
        }
    }
    private void HeadbuttCheck(Character other) 
    {
        if (character.characterType == Character.CharacterType.AI)
            return;

        if (other == null)
            return;

        if (other.headbuttDamage == 0)
            return;

        if (character.characterType == other.characterType)
            return;

        if (other.health.authorizedAbility)
            return;

        DamageCalculate(other.headbuttDamage);
    }
    private bool AuthorizedDamage()
    {
        if (invulnerable
            || invulnerableInfinity)
            return false;

        return true;
    }
    public void DamageCalculate(float damageValue)
    {
        currentHealth -= damageValue;

        switch (character.characterType)
        {
            case Character.CharacterType.Player:
                guiManager.UpdateHealth();
                if (currentHealth <= 0)
                {
                    Destroy();
                    return;
                }
                break;
            case Character.CharacterType.AI:
                if (currentHealth <= 0)
                {
                    if (IsGuardeBreakableHealth)
                    {
                        GuardBreak();
                    }
                    else
                    {
                        FinishObjectFeedback();
                        return;
                    }
                }
                break;
        }
        OnInvincible();
    }

    private void AnimationStateChange(bool changeValue)
    {
        
        character.animator.SetBool(changeAnimationName, changeValue);
        if (changeValue)
        {
            character.characterCondition = Character.CharacterCondition.Stunned;
            character.characterMoveState = Character.CharacterMoveState.Idle;
        }
        else
            character.characterCondition = Character.CharacterCondition.Normal;

    }
    private void OnInvincible()
    {
        invulnerable = true;

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        switch (character.characterType)
        {
            case Character.CharacterType.Player:
                AnimationStateChange(invulnerable);
                if (LevelManager.Instance.IsReturnPosition)
                {
                    LevelManager.Instance.FadeEvent(LevelManager.FadeState.FadeInOut, 0.3f);
                    character.velocity = Vector2.zero;
                    character.gravityScale = 0f;
                }
                else if (character.IsSand)
                {
                    character.velocity *= -0.3f * IsForwardKnockback;
                }
                else
                {
                    LevelManager.Instance.TimeEvent(0.2f, 0f);
                    LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
                    character.velocity = knockbackForce;
                    character.gravityScale = hitGravityScale;
                    character.velocity.x *= IsForwardKnockback;
                }
                
                coroutine = StartCoroutine(InvincibleTime());
                break;
            case Character.CharacterType.AI:
                coroutine = StartCoroutine(InvincibleTimeAi());
                CheckDistanceToPlayer();
                break;
        }
    }
    private void CheckDistanceToPlayer()
    {
        if (character.transform.position.x - LevelManager.Instance.playerCharacter.transform.position.x > 0)
        {
            if (character.velocity.x < 0)
                character.velocity.x *= -1f;
        }
        else
        {
            if (character.velocity.x > 0)
                character.velocity.x *= -1f;
        }
    }
    public void Destroy()
    {
        LevelManager.Instance.TimeEvent(finishAttackWaitTime, 0f);
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        GameObject feedback = Instantiate(deathFeedBack);
        feedback.transform.position = transform.position;

        if (character.characterType == Character.CharacterType.Player)
        {
            LevelManager.Instance.RespawnPlayer();
            gameObject.SetActive(false);
            return;
        }
        OffAttackPoint();
        gameObject.SetActive(false);
    }
    private void GuardBreak()
    {
        AnimationStateChange(true);
        authorizedAbility = true;
        guardBreakPointAnim.SetBool("GuardBreak", true);

        character.aiBrain.InitializedAi();
        invulnerableBuffer = guardBreakTime;
        character.velocity = knockbackForce;
        character.gravityScale = hitGravityScale;
        character.velocity.x *= character.transform.localScale.x;
        StartCoroutine(ReturnKncokbackForce());
    }
    private IEnumerator ReturnKncokbackForce()
    {
        var waitTime = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitTime;
            character.velocity = Vector2.MoveTowards(character.velocity, Vector2.zero, Time.deltaTime * 10f);
            if ((Mathf.Abs(character.velocity.x) <= 0.05f && Mathf.Abs(character.velocity.y) <= 0.05f) || !gameObject.activeSelf)
            {
                break;
            }
        }
        character.velocity = Vector2.zero;
    }
    private IEnumerator InvincibleTime()
    {
        var loopTime = new WaitForFixedUpdate();

        yield return loopTime;
        character.IsSand = false;
        character.characterDig.IsDigStop = false;
        float curTime = 0f;
        
        while (true)
        {
            yield return loopTime;
            curTime += Time.deltaTime;
            if (curTime >= 0.3f)
            {
                break;
            }
        }

        if (LevelManager.Instance.IsReturnPosition)
        {
            LevelManager.Instance.IsReturnPosition = false;
            character.transform.position = LevelManager.Instance.currentReturnPosition.transform.position;
        }

        AnimationStateChange(false);
        character.gravityScale = character.initialGravityScale;
        character.model.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        character.model.GetComponent<SpriteRenderer>().DOFade(1f, invulnerableBuffer * 0.1f).SetLoops(9, LoopType.Yoyo);
        yield return new WaitForSeconds(invulnerableBuffer);

        if (character.characterMoveState != Character.CharacterMoveState.Dash)
        {
            invulnerable = false;
        }
    }

    private IEnumerator InvincibleTimeAi()
    {
        sprite.color = Color.red;
        character.model.GetComponent<SpriteRenderer>().DOColor(Color.white, invulnerableBuffer);
        GameObject feedback = Instantiate(hitFeedback);
        feedback.transform.position = transform.position;

        LevelManager.Instance.TimeEvent(0.075f, 0f);
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
        base.StartHitFeedback();

        yield return new WaitForSeconds(invulnerableBuffer);
        character.gravityScale = character.initialGravityScale;
        invulnerable = false;

        if (authorizedAbility)
        {
            guardBreakPointAnim.SetBool("GuardBreak", false);
            character.aiBrain.StartBrain();
            OffAttackPoint();
            InitializedHealth();
        }
        
        AnimationStateChange(invulnerable);
    }
}
