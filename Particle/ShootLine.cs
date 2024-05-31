using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLine : MonoBehaviour
{
    [SerializeField] private Material[] changeMaterials;
    private LineRenderer lineRenderer;
    private Material material;
    private WaitForSeconds waitTime;
    [HideInInspector] public int count = 0;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        material = GetComponent<Material>();
        waitTime = new WaitForSeconds(0.1f);
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(UpdateMaterial());
    }
    private IEnumerator UpdateMaterial()
    {
        while (true)
        {
            yield return waitTime;
            lineRenderer.sharedMaterial = changeMaterials[count];
            if (count < changeMaterials.Length - 1)
            { 
                count++;
            }
            else
            {
                count = 0;
                break;
            }
        }

        lineRenderer.gameObject.SetActive(false);
    }
}
