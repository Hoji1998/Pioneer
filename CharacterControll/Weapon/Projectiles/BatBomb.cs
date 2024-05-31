using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatBomb : Projectile
{
    [Header("FireEffect")]
    [SerializeField] private GameObject fireParticlePoint;
    [SerializeField] private GameObject fireParticle;
    [Header("DestroyEffect")]
    [SerializeField] private GameObject destroyEffect;
    [Header("Acceleration Component")]
    [SerializeField] private float accelerationValue = 1f;
    [SerializeField] private float maxSpeed = 8f;
    private GameObject InstanceParticle;
    public override void LaunchOfProjectile(Vector3 startTr)
    {
        base.LaunchOfProjectile(startTr);
        InstanceParticle = Instantiate(fireParticle, fireParticlePoint.transform);
        InstanceParticle.transform.parent = this.transform;
    }
    public override void AddProjectileMove()
    {
        velocity += Vector2.down * speed * Time.deltaTime * accelerationValue;
        accelerationValue += 1f;

        if (velocity.y < -maxSpeed)
        {
            velocity.y = -maxSpeed;
        }
    }
    public override void DestroyProjectile()
    {
        GameObject instanceEffect = Instantiate(destroyEffect, this.gameObject.transform);
        instanceEffect.transform.parent = null;
        accelerationValue = 1f;
        InstanceParticle.transform.parent = null;
        InstanceParticle.GetComponent<SelfDestroy>().DestroyParticleStart(0.3f);
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
        base.DestroyProjectile();
    }
}
