using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiExplosion : AiAbility
{
    [Header("Explosion Component")]
    [SerializeField] private float trackingSpeed = 3f;
    [SerializeField] private float explosionTime = 3.0f;
    [SerializeField] private GameObject explosionFeedback;

    private bool IsFoward = false;
    private bool IsExplosion = false;
    private Coroutine coroutine;
    public override void Start()
    {
        character = GetComponent<Character>();
        brain = GetComponent<AiBrain>();
    }
    public override void InitializedAbility()
    {
        base.InitializedAbility();
        
        StopAllCoroutines();
        AnimationStateChange(false);
        IsExplosion = false;
    }
    public override void ProcessAbility()
    {
        if (!AuthorizedAbility())
            return;

        AbilityStart();
    }
    private IEnumerator ExplosionTracking()
    {
        var loop = new WaitForFixedUpdate();
        character.velocity.x = 0f;
        float curTime = 0f;
        while (true)
        {
            yield return loop;
            if (character.characterCondition == Character.CharacterCondition.Stunned)
            {
                StopAllCoroutines();
            }
            curTime += Time.deltaTime;
            ChasingPlayer();

            if (curTime >= explosionTime)
            {
                break;
            }
        }
        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
        Instantiate(explosionFeedback, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
    public override void AbilityStart()
    {
        IsExplosion = true;
        AnimationStateChange(true);
        coroutine = StartCoroutine(ExplosionTracking());
    }
    private void ChasingPlayer()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        IsFoward = character.transform.position.x - playerCharacter.transform.position.x < 0 ? true : false;

        switch (IsFoward)
        {
            case true:
                character.velocity.x += trackingSpeed * Time.deltaTime;
                character.horizontalSpeed = trackingSpeed;

                if (character.velocity.x > trackingSpeed)
                {
                    character.velocity.x = trackingSpeed;
                }
                break;
            case false:
                character.velocity.x += trackingSpeed * -1f * Time.deltaTime;
                character.horizontalSpeed = trackingSpeed * -1f;

                if (character.velocity.x < -trackingSpeed)
                {
                    character.velocity.x = -trackingSpeed;
                }
                break;
        }
    }
    public override bool AuthorizedAbility()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned ||
            character.characterMoveState == Character.CharacterMoveState.Jumping ||
            character.characterMoveState == Character.CharacterMoveState.Falling ||
            IsExplosion)
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
