using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Crane : FinishAttackPoint
{
    [Header("Crane Component")]
    [SerializeField] private LineRenderer craneLine;
    [SerializeField] private GameObject nextCranePoint;
    [SerializeField] private GameObject mainCranePoint;
    [SerializeField] private float speed = 3f;
    [SerializeField] private Animator[] animators;

    private Character character;
    private Coroutine coroutine;
    private Coroutine returnCoroutine;
    private enum CraneDirection { Forward, Backward, Stop }
    private CraneDirection currentCraneDirection;
    public override void Start()
    {
        base.Start();
        gameObject.GetComponent<SpriteRenderer>().flipX = mainCranePoint.transform.position.x - nextCranePoint.transform.position.x < 0 ? false : true;
        nextCranePoint.GetComponent<SpriteRenderer>().flipX = mainCranePoint.transform.position.x - nextCranePoint.transform.position.x < 0 ? false : true;
        mainCranePoint.GetComponent<SpriteRenderer>().flipX = mainCranePoint.transform.position.x - nextCranePoint.transform.position.x < 0 ? false : true;
    }
    public override void OnDrawGizmos()
    {
#if UNITY_EDITOR
        base.OnDrawGizmos();
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, nextCranePoint.transform.position);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(nextCranePoint.transform.position, 0.5f);
        craneLine.SetPosition(1, nextCranePoint.transform.localPosition);
    #endif
    }
    private void OnEnable()
    {
        StartCoroutine(CraneLineAnimation());
        transform.position = mainCranePoint.transform.position;
        currentCraneDirection = CraneDirection.Stop;
    }
    public override void FinishObjectFeedback()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        base.StartHitFeedback();

        character.characterMoveState = Character.CharacterMoveState.Hanging;
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        coroutine = StartCoroutine(CraneStart());
    }
    private IEnumerator CraneLineAnimation()
    {
        var waitTime = new WaitForFixedUpdate();
        float curTime = 0f;

        while (true)
        {
            yield return waitTime;
            curTime += Time.deltaTime;

            switch (currentCraneDirection)
            {
                case CraneDirection.Forward:
                    craneLine.material.SetTextureOffset("_MainTex", Vector2.left * speed * curTime);
                    foreach (Animator anim in animators)
                    {
                        anim.SetFloat("Rotate", -1.0f);
                        anim.SetFloat("Reverse", 1.0f);
                        anim.speed = 1f;
                    }
                    break;
                case CraneDirection.Backward:
                    craneLine.material.SetTextureOffset("_MainTex", Vector2.right * speed * curTime);
                    foreach (Animator anim in animators)
                    {
                        anim.SetFloat("Rotate", 1.0f);
                        anim.SetFloat("Reverse", 1.0f);
                        anim.speed = 1f;
                    }
                    break;
                case CraneDirection.Stop:
                    foreach (Animator anim in animators)
                    {
                        anim.speed = 0f;
                    }
                    break;
            }
        }
    }
    private IEnumerator CraneStart()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        
        character.velocity = Vector2.zero;
        currentCraneDirection = CraneDirection.Forward;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            character.transform.position = Vector3.MoveTowards(character.transform.position, nextCranePoint.transform.position, speed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, nextCranePoint.transform.position, speed * Time.deltaTime);

            if (transform.position == nextCranePoint.transform.position)
            {
                currentCraneDirection = CraneDirection.Stop;
            }


            if (character.characterMoveState != Character.CharacterMoveState.Hanging || character.characterCondition == Character.CharacterCondition.Stunned)
            {
                returnCoroutine = StartCoroutine(CraneReturn());
                break;
            }
        }
    }
    private IEnumerator CraneReturn()
    {
        currentCraneDirection = CraneDirection.Backward;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            transform.position = Vector3.MoveTowards(transform.position, mainCranePoint.transform.position, speed * Time.deltaTime);

            if (transform.position == mainCranePoint.transform.position)
            {
                currentCraneDirection = CraneDirection.Stop;
                break;
            }
        }
    }
}
