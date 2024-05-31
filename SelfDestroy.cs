using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [Header("DestroyTime")]
    public float destroyTime = 2f;

    private void OnEnable()
    {
        if (destroyTime <= 0f)
            return;

        DestroyParticleStart(destroyTime);
    }
    public void DestroyParticleStart(float value)
    {
        Invoke("DestroyParticle", value);
    }
    private void DestroyParticle()
    {
        Destroy(gameObject);
    }
}
