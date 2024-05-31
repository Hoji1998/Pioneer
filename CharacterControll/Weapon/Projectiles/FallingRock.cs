using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRock : Projectile
{
    private CollisionChecker2D collisionChecker2D;
    private float InitDamage = 0f;
    public override void Start()
    {
        base.Start();
        InitDamage = damage;
    }
    public override void AddProjectileMove()
    {
        collisionChecker2D.Reset();
        velocity.y = speed;
    }
    public override void RaycastVertical(ref Vector3 velocity)
    {
        float direction = Mathf.Sign(velocity.y);
        float distance = Mathf.Abs(velocity.y) + skinwidth;
        Vector2 rayPosition = Vector2.zero;
        RaycastHit2D hit;
        this.gameObject.layer = 14;
        IsPlayer = false;
        damage = InitDamage;
        for (int i = 0; i < verticalRayCount; ++i)
        {
            rayPosition = direction == 1 ? colliderCorner2D.topLeft : colliderCorner2D.bottomLeft;
            rayPosition += Vector2.right * (verticalRaySpacing * i + velocity.x);

            //±¤¼± ¹ß»ç
            hit = Physics2D.Raycast(rayPosition, Vector2.up * direction, distance, collisionLayer);


            //±¤¼±¿¡ ºÎµúÈû
            if (hit)
            {
                velocity.y = 0f;
                distance = hit.distance;

                collisionChecker2D.down = direction == -1;
                collisionChecker2D.up = direction == 1;
                this.gameObject.layer = 3;
                IsPlayer = true;
                damage = 0f;
            }
        }
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
