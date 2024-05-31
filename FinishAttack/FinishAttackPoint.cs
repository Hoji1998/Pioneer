using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FinishAttackPoint : MonoBehaviour
{
    [Header("FinishAttackPoint Component")]
    [SerializeField] private float maximumFinishAttackDistance = 5f;
    [SerializeField] private LineRenderer finishAttackLine;
    [SerializeField] private List<SpriteRenderer> hitSprites;
    public bool authorizedAbility = false;
    [HideInInspector] public bool IsAttackPoint { get => isAttackPoint; }
    [HideInInspector] public float currentFinishAttackDistance = 0f;
    [HideInInspector] public bool IsEnemy;
    [HideInInspector] public Character playerCharacter;

    private bool isAttackPoint = false;
    private RaycastHit2D hit;
    private RaycastHit2D wallHit;
    private Coroutine hitMaterialCoroutine;
    public virtual void OnDrawGizmos()
    {
        if (!authorizedAbility)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maximumFinishAttackDistance);
    }
    public virtual void Start()
    {
        IsEnemy = GetComponent<Health>() == null ? false : true;
    }
    public virtual void OnDisable()
    {
        OffAttackPoint();
    }
    public virtual void FinishObjectFeedback()
    {
        Debug.Log("FinishObject Feedback Start!");
    }
    public virtual void Update()
    {
        if (!AuthorizedFinishAttack())
            return;

        FinishAttackRaycast();
    }
    private bool AuthorizedFinishAttack()
    {
        if (!authorizedAbility)
        {
            return false;
        }

        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        return true;
    }
    private void FinishAttackRaycast()
    {
        hit = Physics2D.Raycast(transform.position, playerCharacter.transform.position - transform.position, maximumFinishAttackDistance, playerCharacter.playerLayer);
        finishAttackLine.SetPosition(0, transform.position);
        finishAttackLine.SetPosition(1, playerCharacter.transform.position);
        InitializedFinishLine();

        if (hit)
        {
            OnAttackPoint();
        }
        else
        {
            OffAttackPoint();
        }
    }
    private void InitializedFinishLine()
    {
        if (finishAttackLine == null)
            return;

        if (!finishAttackLine.gameObject.activeSelf)
        {
            finishAttackLine.gameObject.SetActive(true);
        }
        UpdateFinishAttackLine(new Color(0f, 0f, 0f, 0f));
    }
    private bool WallCheck()
    {
        wallHit = Physics2D.Raycast(transform.position, playerCharacter.transform.position - transform.position,
            Vector2.Distance(transform.position, playerCharacter.transform.position), playerCharacter.collisionLayer - playerCharacter.onewayPlatformLayer);
        if (wallHit)
        {
            return false;
        }

        return true;
    }
    private void OnAttackPoint()
    {
        if (!WallCheck())
        {
            OffAttackPoint();
            return;
        }

        if (!isAttackPoint)
        {
            playerCharacter.characterWeaponAttack.finishAttackObjects.Add(this);
        }

        if (playerCharacter.characterWeaponAttack.CurrentFinishAttackObject == this)
        {
            UpdateFinishAttackLine(Color.red);
        }
        else
        {
            UpdateFinishAttackLine(new Color(1f, 0f, 0f, 0.2f));
        }
        
        isAttackPoint = true;
        currentFinishAttackDistance = Vector2.Distance(transform.position, playerCharacter.transform.position);
    }
    public virtual void OffAttackPoint()
    {
        isAttackPoint = false;
        if (playerCharacter != null)
        {
            playerCharacter.characterWeaponAttack.finishAttackObjects.Remove(this);
        }

        InitializedFinishLine();
    }
    public virtual void UpdateFinishAttackLine(Color color)
    {
        finishAttackLine.startColor = color;
        finishAttackLine.endColor = color;
    }
    public virtual void StartHitFeedback()
    {
        if (hitSprites.Count == 0)
            return;

        if (hitMaterialCoroutine != null)
            StopCoroutine(hitMaterialCoroutine);

        hitMaterialCoroutine = StartCoroutine(MaterialColorChange());
    }
    private IEnumerator MaterialColorChange()
    {
        float curValue = 0.8f;
        var loop = new WaitForFixedUpdate();
        while (curValue >= 0f)
        {
            yield return loop;
            curValue -= Time.deltaTime * 5f;
            foreach (SpriteRenderer hitSprite in hitSprites)
            {
                hitSprite.material.SetColor("_GlowColor", new Color(curValue, curValue, curValue, 0f));
            }
        }

        foreach (SpriteRenderer hitSprite in hitSprites)
        {
            hitSprite.material.SetColor("_GlowColor", new Color(0f, 0f, 0f, 0f));
        }
    }
}
