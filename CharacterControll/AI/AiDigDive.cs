using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AiDigDive : AiAbility
{
    [Header("DigDive Component")]
    [SerializeField] private float digHeight = 10f;
    [SerializeField] private float diveRange = 10f;
    [SerializeField] private float diveDelay = 1f;
    [SerializeField] private int maxDigDiveCount = 1;

    [Header("DigGround Setting")]
    [SerializeField] private GameObject groundPos;
    [SerializeField] private GameObject leftLimitPos;
    [SerializeField] private GameObject rightLimitPos;
    //[SerializeField] private GameObject diveCompletePos;
    //[SerializeField] private GameObject diveLandingPos;

    [Header("FrictionEffect")]
    [SerializeField] private GameObject frictionEffect;
    [SerializeField] private GameObject frictionEffectPos;

    [Header("DiggingEffect")]
    [SerializeField] private GameObject sandSplashEffect;
    [SerializeField] private GameObject sandSplashEffectPos;
    [SerializeField] private GameObject rockSplashEffect;

    [Header("DiggingSplashAttack Component")]
    [SerializeField] private GameObject splashAttack;
    [SerializeField] private float splashAttackDamage = 10f;
    [SerializeField] private int multipleShot = 5;
    [SerializeField] private int poolSize = 15;

    [HideInInspector] public bool IsDigLanding = true;

    private Vector2 diveStartPos = Vector2.zero;
    private Vector2 diveEndPos = Vector2.zero;
    private float height = 0;
    private enum DiveDirection { Left, Right }
    private DiveDirection currentDiveDirection;
    private List<Projectile> projectiles;
    private BreakableObject[] rockSplashEffects;
    private bool IsDigDive = false;
    private int currentDigDiveCount = 0;
    private float timer = 0f;
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (leftLimitPos == null)
            return;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontSize = 40;
        style.fontStyle = FontStyle.Bold;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(leftLimitPos.transform.position, 1.5f);
        Gizmos.DrawWireSphere(rightLimitPos.transform.position, 1.5f);
#endif
    }
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
        rockSplashEffects = rockSplashEffect.GetComponentsInChildren<BreakableObject>();

        CreateProjectilePool();
    }
    private void CreateProjectilePool()
    {
        projectiles = new List<Projectile>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject instanceObject = Instantiate(splashAttack);
            instanceObject.SetActive(false);
            projectiles.Add(instanceObject.GetComponent<Projectile>());
        }
    }
    private void LaunchOfProjectile(float divideValue, float currentValue)
    {
        foreach (Projectile instanceProjectile in projectiles)
        {
            if (!instanceProjectile.gameObject.activeSelf)
            {
                instanceProjectile.damage = splashAttackDamage;
                instanceProjectile.LaunchOfProjectile(new Vector2(transform.position.x, sandSplashEffectPos.transform.position.y));

                if (playerCharacter.transform.position.x - transform.position.x < 0)
                {
                    instanceProjectile.velocity += new Vector2(UnityEngine.Random.Range(-currentValue - divideValue, -currentValue), 
                        (UnityEngine.Random.Range(5f + currentValue, 5f + currentValue + divideValue)));
                }
                else
                {
                    instanceProjectile.velocity += new Vector2(UnityEngine.Random.Range(currentValue, currentValue + divideValue),
                        (UnityEngine.Random.Range(5f + currentValue, 5f + currentValue + divideValue)));
                }
                
                instanceProjectile.gameObject.SetActive(true);
                break;
            }
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        DigDiveComplete();

        //objectSpawner.SpawnStop();
        character.transform.localScale = Vector2.one;
        IsDigLanding = true;
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        currentDigDiveCount = 0;
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    public override void AbilityStart()
    {
        if (IsDigLanding)
        {
            StartCoroutine(IntroDive());
            return;
        }
        
        IsDigDive = true;
        brain.UpdateBrainWeight();

        StartCoroutine(DigDive());
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsDigDive)
        {
            return false;
        }

        if (!brain.IsEngaged)
        {
            brain.AbilityChange();
            return false;
        }
        return true;
    }
    private IEnumerator DigDive()
    {
        AnimationStateChange(true);
        var loop = new WaitForFixedUpdate();
        float curTime = 0f;
        while (curTime <= 1.5f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        character.animator.SetTrigger("RushAttack_UnderAttack");
        LevelManager.Instance.ShakeCameraEvent(5.01f, 2f);
        CreateRockSplashEffect();

        if (character.transform.localScale.x < 0)
        {
            diveStartPos = new Vector2(transform.position.x + diveRange, groundPos.transform.position.y - digHeight);
            diveEndPos = new Vector2(transform.position.x - diveRange, diveStartPos.y);
        }
        else
        {
            diveStartPos = new Vector2(transform.position.x - diveRange, groundPos.transform.position.y - digHeight);
            diveEndPos = new Vector2(transform.position.x + diveRange, diveStartPos.y);
        }

        timer = 0.5f;
        float height = (groundPos.transform.position.y - diveStartPos.y);
        float timerAcceleration = 0f;
        curTime = 0f;

        bool IsStartSplash = false;

        while (timer < 1f)
        {
            timerAcceleration = Mathf.Abs(timer - 0.5f) + 0.05f;
            timer += Time.deltaTime * timerAcceleration;

            pastPosition = character.transform.position;
            Vector2 tempPos = Parabola(diveStartPos, diveEndPos, height, timer);
            curTime += Time.deltaTime;

            //sand splash Effect
            if (character.transform.position.y >= sandSplashEffectPos.transform.position.y
                && !IsStartSplash)
            {
                IsStartSplash = true;
                CreateDiggingEffect();
            }

            character.transform.position = tempPos;
            ModelRotate(tempPos - pastPosition);
            yield return loop;
        }

        curTime = 0f;
        while (curTime <= diveDelay)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        StartCoroutine(DiveAttack());
    }
    private void DivePositionCalculate(DiveDirection diveDirection)
    {
        switch (diveDirection)
        {
            case DiveDirection.Right:
                character.transform.localScale = new Vector2(-1f, 1f);
                diveStartPos = new Vector2(playerCharacter.transform.position.x + diveRange, groundPos.transform.position.y - digHeight);
                diveEndPos = new Vector2(playerCharacter.transform.position.x - diveRange, diveStartPos.y);

                if (diveStartPos.x >= rightLimitPos.transform.position.x)
                {
                    diveStartPos.x = rightLimitPos.transform.position.x;
                    diveEndPos.x = rightLimitPos.transform.position.x - (diveRange * 2);
                }

                if (diveEndPos.x <= leftLimitPos.transform.position.x)
                {
                    diveStartPos.x = leftLimitPos.transform.position.x + (diveRange * 2);
                    diveEndPos.x = leftLimitPos.transform.position.x;
                }
                break;
            case DiveDirection.Left:
                character.transform.localScale = new Vector2(1f, 1f);
                diveStartPos = new Vector2(playerCharacter.transform.position.x - diveRange, groundPos.transform.position.y - digHeight);
                diveEndPos = new Vector2(playerCharacter.transform.position.x + diveRange, diveStartPos.y);

                if (diveStartPos.x <= leftLimitPos.transform.position.x)
                {
                    diveStartPos.x = leftLimitPos.transform.position.x;
                    diveEndPos.x = leftLimitPos.transform.position.x + (diveRange * 2);
                }

                if (diveEndPos.x >= rightLimitPos.transform.position.x)
                {
                    diveStartPos.x = rightLimitPos.transform.position.x - (diveRange * 2);
                    diveEndPos.x = rightLimitPos.transform.position.x;
                }
                break;
        }
    }
    private static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, f(t) + Mathf.Lerp(start.y, end.y, t));
    }
    private IEnumerator DiveAttack()
    {
        AnimationStateChange(true);
        CreateRockSplashEffect();
        currentDigDiveCount++;
        diveStartPos = Vector2.zero;
        diveEndPos = Vector2.zero;
        currentDiveDirection = currentDigDiveCount % 2 == 0 ? DiveDirection.Left : DiveDirection.Right;

        var loop = new WaitForFixedUpdate();
        float curTime = 0;
        while (true)
        {
            yield return loop;
            curTime += Time.deltaTime;
            DivePositionCalculate(currentDiveDirection);
            character.transform.position = diveStartPos;

            if (curTime >= diveDelay)
            {
                break;
            }
        }
        height = (playerCharacter.transform.position.y - diveStartPos.y + 3f);
        //미리 출발위치로 이동후 대기
        StartCoroutine(DiveAttackStart());
    }

    private IEnumerator DiveAttackStart()
    {
        character.transform.position = diveStartPos;
        //Maximum height
        if (character.transform.position.y + height > groundPos.transform.position.y + 5)
        {
            height -= (character.transform.position.y + height) - (groundPos.transform.position.y + 5);
        }

        timer = 0f;
        bool IsStartSplash = false;
        bool IsEndSplash = false;
        var loop = new WaitForFixedUpdate();
        float timerAcceleration = 0f;
        while (character.transform.position.y >= diveStartPos.y)
        {
            timerAcceleration = Mathf.Abs(timer - 0.5f) + 0.3f;
            timer += Time.deltaTime * timerAcceleration;

            pastPosition = character.transform.position;

            Vector2 tempPos = Parabola(diveStartPos, diveEndPos, height, timer);
            character.transform.position = tempPos;
            ModelRotate(tempPos - pastPosition);

            //sand splash Effect
            if (character.transform.position.y >= sandSplashEffectPos.transform.position.y
                && !IsStartSplash)
            {
                IsStartSplash = true;
                SpawnSplashAttack();
                LevelManager.Instance.ShakeCameraEvent(7f, 0.3f);
                CreateDiggingEffect();
            }

            if (character.transform.position.y <= sandSplashEffectPos.transform.position.y
                && IsStartSplash
                && !IsEndSplash)
            {
                IsEndSplash = true;
                LevelManager.Instance.ShakeCameraEvent(7f, 0.3f);
                CreateDiggingEffect();
            }

            yield return loop;
        }

        //objectSpawner.SpawnStart();
        float curTime = 0f;
        while (curTime <= diveDelay)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        if (currentDigDiveCount >= maxDigDiveCount)
        {
            //objectSpawner.SpawnStop();
            curTime = 0f;
            while (curTime <= diveDelay * 2f)
            {
                yield return loop;
                curTime += Time.deltaTime;
            }
            StartCoroutine(DiveLandingPosition());
        }
        else
        {
            StartCoroutine(DiveAttack());
        }

    }
    private IEnumerator IntroDive()
    {
        AnimationStateChange(true);
        character.velocity = Vector2.zero;
        character.transform.localScale = Vector2.one;
        IsDigLanding = false;

        character.animator.SetTrigger("RushAttack_UnderAttack");
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        CreateRockSplashEffect();
        LevelManager.Instance.ShakeCameraEvent(5.01f, 2f);
        currentDiveDirection = character.transform.localScale.x > 0
            ? DiveDirection.Left
            : DiveDirection.Right;

        DivePositionCalculate(currentDiveDirection);
        diveStartPos.x -= Mathf.Abs(playerCharacter.transform.position.x - transform.position.x) - 5f;
        diveEndPos.x -= Mathf.Abs(playerCharacter.transform.position.x - transform.position.x) - 5f;

        character.transform.position = diveStartPos;
        character.model.transform.rotation = Quaternion.identity;
        timer = 0f;
        float height = Mathf.Abs(groundPos.transform.position.y - diveEndPos.y);
        float timerAcceleration = 0.55f;
        float curTime = 0f;
        var loop = new WaitForFixedUpdate();
        bool IsStartSplash = false;

        while (timer <= 0.5f)
        {
            timerAcceleration = 0.55f - timer;
            timer += Time.deltaTime * timerAcceleration;

            pastPosition = character.transform.position;
            Vector2 tempPos = Parabola(diveStartPos, diveEndPos, height, timer);
            if (timer >= 0.5f)
            {
                tempPos.y = groundPos.transform.position.y;
                rockSplashEffect.SetActive(false);
                if (curTime >= 0.1f)
                {
                    CreatefrictionEffect();
                    curTime = 0f;
                }
            }
            curTime += Time.deltaTime;

            //sand splash Effect
            if (character.transform.position.y >= sandSplashEffectPos.transform.position.y
                && !IsStartSplash)
            {
                IsStartSplash = true;
                CreateDiggingEffect();
            }

            character.transform.position = tempPos;
            ModelRotate(tempPos - pastPosition);
            yield return loop;
        }

        character.model.transform.rotation = Quaternion.identity;
        character.transform.position = new Vector3(character.transform.position.x, groundPos.transform.position.y, 0f);
        character.animator.SetTrigger("RushAttack_AttackDone");

        curTime = 0f;
        while (curTime <= 2f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        DigDiveComplete();
        brain.AbilityChange();
    }
    private IEnumerator DiveLandingPosition()
    {
        CreateRockSplashEffect();
        LevelManager.Instance.ShakeCameraEvent(5.01f, 2f);
        character.transform.localScale = new Vector2(character.transform.localScale.x * -1f, 1f);

        currentDiveDirection = character.transform.localScale.x > 0
            ? DiveDirection.Left
            : DiveDirection.Right;

        DivePositionCalculate(currentDiveDirection);
        character.transform.position = diveStartPos;
        character.model.transform.rotation = Quaternion.identity;
        timer = 0f;
        float height = Mathf.Abs(groundPos.transform.position.y - diveEndPos.y);
        float timerAcceleration = 0.55f;
        float curTime = 0f;
        var loop = new WaitForFixedUpdate();
        bool IsStartSplash = false;

        while (timer <= 0.5f)
        {
            timerAcceleration = 0.55f - timer;
            timer += Time.deltaTime * timerAcceleration;

            pastPosition = character.transform.position;
            Vector2 tempPos = Parabola(diveStartPos, diveEndPos, height, timer);
            if (timer >= 0.5f)
            {
                tempPos.y = groundPos.transform.position.y;
                rockSplashEffect.SetActive(false);
                if (curTime >= 0.1f)
                {
                    CreatefrictionEffect();
                    curTime = 0f;
                }
            }
            curTime += Time.deltaTime;

            //sand splash Effect
            if (character.transform.position.y >= sandSplashEffectPos.transform.position.y
                && !IsStartSplash)
            {
                IsStartSplash = true;
                CreateDiggingEffect();
            }

            character.transform.position = tempPos;
            ModelRotate(tempPos - pastPosition);
            yield return loop;
        }

        character.model.transform.rotation = Quaternion.identity;
        character.transform.position = new Vector3(character.transform.position.x, groundPos.transform.position.y, 0f);
        character.animator.SetTrigger("RushAttack_AttackDone");

        curTime = 0f;
        while (curTime <= 2f)
        {
            yield return loop;
            curTime += Time.deltaTime;
        }

        DigDiveComplete();
        brain.AbilityChange();
    }
    private Vector2 pastPosition;
    private void ModelRotate(Vector2 drillDirection)
    {
        float drillAngle = Mathf.Atan2(drillDirection.y, drillDirection.x) * Mathf.Rad2Deg;
        Quaternion angleAxis_model = Quaternion.identity;

        switch (character.transform.localScale.x)
        {
            case -1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle + 180f, Vector3.forward);
                break;
            case 1:
                angleAxis_model = Quaternion.AngleAxis(drillAngle, Vector3.forward);
                break;
        }


        Quaternion modelRotation = Quaternion.Slerp(character.model.transform.rotation, angleAxis_model, 1f);
        character.model.transform.rotation = modelRotation;
    }
    private void CreatefrictionEffect()
    {
        GameObject instanceEffect = Instantiate(frictionEffect);
        instanceEffect.transform.position = frictionEffectPos.transform.position;
        instanceEffect.transform.rotation = frictionEffectPos.transform.rotation;
        instanceEffect.transform.localScale = this.gameObject.transform.localScale;
    }
    private void CreateDiggingEffect()
    {
        GameObject instanceEffect = Instantiate(sandSplashEffect);
        instanceEffect.transform.position = new Vector2(transform.position.x, sandSplashEffectPos.transform.position.y);
    }
    private void SpawnSplashAttack()
    {
        float divideValue = 1.5f;
        float currentValue = 1.5f;
        for (int i = 0; i < multipleShot; i++)
        {
            LaunchOfProjectile(divideValue, currentValue);
            currentValue += divideValue;
        }
    }
    private void CreateRockSplashEffect()
    {
        rockSplashEffect.SetActive(true);

        foreach (BreakableObject effect in rockSplashEffects)
        {
            effect.InitializeBreakableObject();
        }
    }
    private void DigDiveComplete()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }
        character.model.transform.rotation = Quaternion.identity;
        AnimationStateChange(false);
        IsDigDive = false;
        currentDigDiveCount = 0;
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
