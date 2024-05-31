using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StoneSpawner : MonoBehaviour
{
    [Header("StoneSpawner Component")]
    [SerializeField] private GameObject crushParticle;
    [SerializeField] List<SpriteRenderer> spawnStones;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private float stoneIntensity = 6f;

    [Header("SplashAttack Component")]
    [SerializeField] private GameObject splashAttack;
    [SerializeField] private GameObject splashPoint;
    [SerializeField] private float splashAttackDamage = 10f;
    [SerializeField] private int multipleShot = 5;
    [SerializeField] private int poolSize = 15;

    private List<Projectile> projectiles;
    private Coroutine coroutine;
    private Character playerCharacter;
    private void Start()
    {
        foreach (SpriteRenderer stone in spawnStones)
        {
            stone.material.SetColor("_GlowColor", new Color(1f * stoneIntensity, 1f * stoneIntensity, 1f * stoneIntensity, 0f));
            stone.gameObject.SetActive(false);
        }
        CreateProjectilePool();
    }
    private void CreateProjectilePool()
    {
        projectiles = new List<Projectile>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject instanceObject = Instantiate(splashAttack);
            instanceObject.SetActive(false);
            projectiles.Add(instanceObject.GetComponent<Projectile>());
        }
    }
    private void LaunchOfProjectile(float divideValue, float currentValue)
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        foreach (Projectile instanceProjectile in projectiles)
        {
            if (!instanceProjectile.gameObject.activeSelf)
            {
                instanceProjectile.damage = splashAttackDamage;
                instanceProjectile.LaunchOfProjectile(new Vector2(splashPoint.transform.position.x, splashPoint.transform.position.y));

                if (playerCharacter.transform.position.x - transform.position.x < 0)
                {
                    instanceProjectile.velocity += new Vector2(Random.Range(-currentValue - divideValue, -currentValue),
                        (Random.Range(5f + currentValue, 5f + currentValue + divideValue)));
                }
                else
                {
                    instanceProjectile.velocity += new Vector2(UnityEngine.Random.Range(currentValue, currentValue + divideValue),
                        (Random.Range(5f + currentValue, 5f + currentValue + divideValue)));
                }

                instanceProjectile.gameObject.SetActive(true);
                break;
            }
        }
    }
    public void SpawnStones()
    {
        foreach (SpriteRenderer stone in spawnStones)
        {
            stone.gameObject.SetActive(true);
            stone.material.SetColor("_GlowColor", new Color(1f * stoneIntensity, 1f * stoneIntensity, 1f * stoneIntensity, 0f));
        }
        ColorChange();
    }
    public void DestroyStones()
    {
        SpawnSplashAttack();
        foreach (SpriteRenderer stone in spawnStones)
        {
            stone.gameObject.SetActive(false);
        }
    }
    public void ColorChange()
    {
        coroutine = StartCoroutine(MaterialColorChange());
    }
    public void HitGround()
    {
        foreach (SpriteRenderer stone in spawnStones)
        {
            Instantiate(crushParticle, stone.transform.position, Quaternion.identity);
        }
        LevelManager.Instance.ShakeCameraEvent(5f, 0.3f);
    }
    private void SpawnSplashAttack()
    {
        float divideValue = 1.5f;
        float currentValue = 1.5f;
        for (int i = 0; i < multipleShot; i++)
        {
            LaunchOfProjectile(divideValue, currentValue);
            currentValue += divideValue;
        }
    }
    private IEnumerator MaterialColorChange()
    {
        float curValue = 1f;
        var loop = new WaitForFixedUpdate();
        while (curValue >= 0f)
        {
            yield return loop;
            curValue -= Time.deltaTime * fadeSpeed;
            foreach (SpriteRenderer hitSprite in spawnStones)
            {
                hitSprite.material.SetColor("_GlowColor", new Color(curValue, curValue, curValue, 0f));
            }
        }

        foreach (SpriteRenderer hitSprite in spawnStones)
        {
            hitSprite.material.SetColor("_GlowColor", new Color(0f, 0f, 0f, 0f));
        }
    }
}
