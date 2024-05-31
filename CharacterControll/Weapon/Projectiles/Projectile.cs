using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Projectile Component")]
    [SerializeField] public bool IsPlayer = false;
    [SerializeField] public bool IsLaunch = false;
    [SerializeField] public bool IsMelee = false;
    [SerializeField] public bool IsDestructible = false;
    public float speed = 5f;
    public float damage = 10f;

    [Header("Physics")]
    public LayerMask collisionLayer;
    [Range(2, 100)] public int horizontalRayCount = 2;
    [Range(2, 100)] public int verticalRayCount = 2;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float skinwidth = 0.015f;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public float horizontalRaySpacing = 0f;
    [HideInInspector] public float verticalRaySpacing = 0f;
    [HideInInspector] public Vector2 targetDirection;
    [HideInInspector] public ColliderCorner2D colliderCorner2D;
    public virtual void Start()
    {
        if (IsMelee)
            return;

        coll = GetComponent<Collider2D>();
    }
    public virtual void LaunchOfProjectile(Vector3 startTr)
    {
        velocity = Vector2.zero;
        transform.position = startTr;
        IsLaunch = true;
    }
    public virtual bool AuthorizedLaunch()
    {
        if (!IsLaunch)
        {
            return false;
        }

        return true;
    }
    private void FixedUpdate()
    {
        if (IsMelee)
            return;
        ProcessLaunch();
    }
    private void ProcessLaunch()
    {
        if (!AuthorizedLaunch())
            return;

        MoveStart();
    }
    public virtual void MoveStart()
    {
        CalculateRaySpacing();
        UpdateColliderCorner2D();
        AddProjectileMove();
        //¼Ó·Â
        Vector3 currentVelocity = velocity * Time.deltaTime;
        if (currentVelocity.x != 0)
        {
            RaycastHorizontal(ref currentVelocity);
        }

        if (currentVelocity.y != 0)
        {
            RaycastVertical(ref currentVelocity);
        }


        transform.position += currentVelocity;
    }
    public virtual void AddProjectileMove()
    {
        //add move
    }
    public virtual void DestroyProjectile()
    {
        IsLaunch = false;

        switch (IsDestructible)
        {
            case false:
                gameObject.SetActive(false);
                break;
            case true:
                Destroy(gameObject);
                break;
        }
    }
    public virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsLaunch)
            return;

        if (IsMelee)
            return;

        Character character = collision.gameObject.GetComponent<Character>();
        SharedHealth sharedHealth = collision.gameObject.GetComponent<SharedHealth>();

        SharedHealthCollideCheck(sharedHealth);

        if (character == null)
            return;

        switch (IsPlayer)
        {
            case true:
                if (character.characterType != Character.CharacterType.AI)
                    return;
                break;
            case false:
                if (character.characterType != Character.CharacterType.Player)
                    return;
                break;
        }
        
        if (character.health.invulnerable || character.health.invulnerableInfinity)
            return;

        character.health.DamageCalculate(damage);
        DestroyProjectile();
    }
    public virtual void SharedHealthCollideCheck(SharedHealth sharedHealth)
    {
        if (sharedHealth == null)
            return;

        if (!IsPlayer)
            return;

        sharedHealth.OriginalHealth.DamageCalculate(damage);
        DestroyProjectile();
    }
    #region raycast
    public virtual void RaycastHorizontal(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.x);
        float distance = Mathf.Abs(velocity.x) + skinwidth;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;

        for (int i = 0; i < horizontalRayCount; ++i)
        {
            rayPosition = direction == 1 ? colliderCorner2D.bottomRight : colliderCorner2D.bottomLeft;
            rayPosition += Vector2.up * horizontalRaySpacing * i;
            

            //±¤¼± ¹ß»ç
            hit = Physics2D.Raycast(rayPosition, Vector2.right * direction, distance, collisionLayer);

            //±¤¼±¿¡ ºÎµúÈû
            if (hit)
            {
                DestroyProjectile();
            }
        }
    }
    public virtual void RaycastVertical(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.y);
        float distance = Mathf.Abs(velocity.y) + skinwidth;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;

        for (int i = 0; i < verticalRayCount; ++i)
        {
            rayPosition = direction == 1 ? colliderCorner2D.topLeft : colliderCorner2D.bottomLeft;
            rayPosition += Vector2.right * (verticalRaySpacing * i + velocity.x);

            //±¤¼± ¹ß»ç
            hit = Physics2D.Raycast(rayPosition, Vector2.up * direction, distance, collisionLayer);

            //±¤¼±¿¡ ºÎµúÈû
            if (hit)
            {
                DestroyProjectile();
            }
        }
    }

    public virtual void CalculateRaySpacing()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinwidth * -2);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    private void UpdateColliderCorner2D()
    {
        Bounds bounds = coll.bounds;
        bounds.Expand(skinwidth * -2);

        colliderCorner2D.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        colliderCorner2D.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        colliderCorner2D.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    public struct ColliderCorner2D
    {
        public Vector2 topLeft;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }
    #endregion
}
