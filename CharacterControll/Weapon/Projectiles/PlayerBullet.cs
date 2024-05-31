using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Projectile
{
    public override void MoveStart()
    {
        AddProjectileMove();
    }
    public override void AddProjectileMove()
    {
        //velocity = targetDirection.normalized * speed;
    }
}
