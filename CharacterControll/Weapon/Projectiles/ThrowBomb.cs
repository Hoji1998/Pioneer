using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBomb : Projectile
{
    [Header("ThrowBomb Component")]
    [SerializeField] private GameObject model;

    [Header("DestroyEffect")]
    [SerializeField] private GameObject destroyEffect;

    [Header("HitEffect")]
    [SerializeField] private GameObject hitEffect;

    private Character playerCharacter;
    private void OnEnable()
    {
        LaunchOfProjectile(transform.position);
    }
    public override void LaunchOfProjectile(Vector3 startTr)
    {
        velocity = Vector2.zero;
        transform.position = startTr;
        IsLaunch = true;

        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        targetDirection = transform.position.x - playerCharacter.transform.position.x < 0 ? Vector2.left : Vector2.right;

        GameObject instanceEffect = Instantiate(hitEffect, this.gameObject.transform);
        instanceEffect.transform.parent = null;
    }
    public override void AddProjectileMove()
    {
        velocity = targetDirection * speed;
        RotateModel(targetDirection.x);
    }
    private void RotateModel(float bounceDirection)
    {
        model.transform.Rotate(new Vector3(0f, 0f, -5f * bounceDirection));
    }
    public override void SharedHealthCollideCheck(SharedHealth sharedHealth)
    {
        if (sharedHealth == null)
            return;

        if (!IsPlayer)
            return;

        sharedHealth.OriginalHealth.DamageCalculate(damage);
        LevelManager.Instance.TimeEvent(0.2f, 0f);
        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
        DestroyProjectile();
    }
    public override void DestroyProjectile()
    {
        GameObject instanceEffect = Instantiate(destroyEffect, this.gameObject.transform);
        instanceEffect.transform.parent = null;
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
        base.DestroyProjectile();
    }
}
