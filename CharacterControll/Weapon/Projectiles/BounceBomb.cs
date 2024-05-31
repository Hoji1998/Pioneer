using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceBomb : Projectile
{
    [Header("BounceBomb Component")]
    [SerializeField] private float bounceForce = 0f;
    [SerializeField] private float gravityScale = 0f;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject model;

    [Header("ThrowBomb")]
    [SerializeField] private GameObject throwBombFeedback;

    [Header("DestroyEffect")]
    [SerializeField] private GameObject destroyEffect;

    //private Character playerCharacter;
    private CollisionChecker2D collisionChecker2D;
    private bool IsBounce = false;
    //private bool IsBounceMotion = false;
    private float curTime = 0.3f;
    public override void Start()
    {
        base.Start();
        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
    }
    public override void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsLaunch)
            return;

        if (IsMelee)
            return;

        AuthorizedThrowBomb(collision);
        //character
        Character character = collision.gameObject.GetComponent<Character>();

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

        if (character.health.invulnerable)
            return;

        character.health.DamageCalculate(damage);
        DestroyProjectile();
    }
    public override void LaunchOfProjectile(Vector3 startTr)
    {
        velocity = Vector2.up * speed * 5f;
        //this.transform.localScale = Vector2.one * 0.3f;
        transform.position = startTr;
         
        IsLaunch = true;
        IsBounce = false;
        //IsBounceMotion = false;
        animator.SetBool("IsBounce", false);
    }
    public override void AddProjectileMove()
    {
        //if (IsBounceMotion)
        //{
        //    CalculateBounceMotion();
        //    return;
        //}

        collisionChecker2D.Reset();
        velocity.y += gravityScale * Time.deltaTime;

        float bounceDirection = targetDirection.normalized.x < 0 ? -1f : 1f;
        velocity.x = IsBounce ? speed * bounceDirection : 0;

        
        //RotateModel(bounceDirection);
    }
    private void CalculateBounceMotion()
    {
        curTime += Time.deltaTime;
        velocity = Vector2.zero;
        if (curTime >= 0.2)
        {
            curTime = 0f;
            //IsBounceMotion = false;
            base.velocity.y = bounceForce;
            animator.SetBool("IsBounce", false);
        }
    }
    private void AuthorizedThrowBomb(Collider2D collision)
    {
        //ThrowBomb
        Weapon weapon = collision.GetComponent<Weapon>() == null ? collision.GetComponentInParent<Weapon>() : collision.GetComponent<Weapon>();

        if (weapon == null)
        {
            return;
        }
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
        GameObject instanceEffect = Instantiate(throwBombFeedback, this.gameObject.transform);
        instanceEffect.transform.parent = null;
        base.DestroyProjectile();
    }
    private void RotateModel(float bounceDirection)
    {
        model.transform.Rotate(new Vector3(0f, 0f, -5f * bounceDirection));
    }
    public override void RaycastVertical(ref Vector3 velocity)
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
                IsBounce = true;
                //IsBounceMotion = true;
                base.velocity.y = bounceForce;
                distance = hit.distance;
                //animator.SetBool("IsBounce", true);

                collisionChecker2D.down = direction == -1;
                collisionChecker2D.up = direction == 1;
                if (collisionChecker2D.up)
                {
                    DestroyProjectile();
                }
            }
        }
    }
    public override void RaycastHorizontal(ref Vector3 velocity)
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
                //DestroyProjectile();
            }
        }
    }

    public override void DestroyProjectile()
    {
        GameObject instanceEffect = Instantiate(destroyEffect, this.gameObject.transform);
        instanceEffect.transform.parent = null;

        base.DestroyProjectile();
    }
    private struct CollisionChecker2D
    {
        public void Reset()
        {
            left = false;
            right = false;
            up = false;
            down = false;
        }
        public bool left;
        public bool right;
        public bool up;
        public bool down;
    }
}
