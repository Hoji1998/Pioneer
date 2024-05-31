using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    public override void AddProjectileMove()
    {
        velocity.x = targetDirection.x * speed;
    }
}
