using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BreakableObject : MonoBehaviour
{
    [Header("BreakableObject")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float initialiHealth;
    [SerializeField] private GameObject model;
    [SerializeField] private bool IsBlind = false;

    [Header("Additional Destruction")]
    [SerializeField] private List<BreakableObject> additionalBreakableObjects;

    [Header("Feedbacks")]
    [SerializeField] private List<SpriteRenderer> hitSprites;
    [SerializeField] private GameObject damageFeedback;
    [SerializeField] private GameObject destroyFeedback;

    [HideInInspector] public GameObject Model { get => model; }
    private Coroutine hitMaterialCoroutine;
    private void Start()
    {
        InitializeBreakableObject();
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (additionalBreakableObjects.Count == 0)
            return;

        Gizmos.color = Color.yellow;
        foreach (BreakableObject breakableObject in additionalBreakableObjects)
        {
            Gizmos.DrawLine(transform.position, breakableObject.transform.position);
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
#endif
    }
    public void InitializeBreakableObject()
    {
        currentHealth = initialiHealth;
        gameObject.SetActive(true);
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (initialiHealth == 0)
            return;

        if (collision.gameObject.layer == 14)
        {
            DamageCalculate(initialiHealth);
            return;
        }

        if (IsBlind)
            return;

        Weapon weapon = collision.GetComponent<Weapon>() == null ? collision.GetComponentInParent<Weapon>() : collision.GetComponent<Weapon>();

        if (weapon == null) 
        {
            return;
        }
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        DamageCalculate(weapon.weaponDamage);
    }

    public void DamageCalculate(float damage)
    {
        StartHitFeedback();
        currentHealth -= damage;
        

        if (currentHealth <= 0)
        {
            if (additionalBreakableObjects.Count != 0)
            {
                foreach (BreakableObject additionalObject in additionalBreakableObjects)
                {
                    additionalObject.DamageCalculate(100);
                }
            }

            Instantiate(destroyFeedback, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
    private void StartHitFeedback()
    {
        Instantiate(damageFeedback, transform.position, Quaternion.identity);

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
