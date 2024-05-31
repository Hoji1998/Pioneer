using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSpawner : MonoBehaviour
{
    [Header("ObjectSpawner Component")]
    public List<SpawnableObject> objects;
    public bool IsLoop = false;
    public bool IsSpawn = false;

    [Serializable] public struct SpawnableObject
    {
        public GameObject obj;
        public Transform tr;
        public float duration;
    }
    private Coroutine coroutine;
    float minDuration;
    float maxDuration;
    bool IsRandomTransform = false;
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (objects.Count == 0)
            return;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;

        int i = 0;
        foreach (SpawnableObject spawnableObject in objects)
        {
            i++;
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(spawnableObject.tr.transform.position, i.ToString() + "\n" + spawnableObject.duration.ToString(), style);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnableObject.tr.transform.position, 1.5f);
        }
        
#endif
    }
    public void SpawnStart()
    {
        IsSpawn = true;
        coroutine = StartCoroutine(Spawn());
    }
    public void SpawnStop()
    {
        IsSpawn = false;

        if (coroutine == null)
            return;

        StopCoroutine(coroutine);
    }
    public void RandomSeedSpawnStart(float minDuration, float maxDuration, bool IsRandomTransform, bool IsLoop)
    {
        this.minDuration = minDuration;
        this.maxDuration = maxDuration;
        this.IsRandomTransform = IsRandomTransform;
        this.IsLoop = IsLoop;

        IsSpawn = true;
        coroutine = StartCoroutine(RandomSeedSpawn());
    }
    private IEnumerator RandomSeedSpawn()
    {
        while (true)
        {
            foreach (SpawnableObject spawnableObject in objects)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minDuration, maxDuration));
                GameObject InstanceObject = Instantiate(spawnableObject.obj, objects[UnityEngine.Random.Range(0, objects.Count - 1)].tr.position, Quaternion.identity);
            }

            if (!IsLoop)
                break;
        }
    }
    private IEnumerator Spawn()
    {
        while (true)
        {
            foreach (SpawnableObject spawnableObject in objects)
            {
                yield return new WaitForSeconds(spawnableObject.duration);
                GameObject InstanceObject = Instantiate(spawnableObject.obj, spawnableObject.tr.transform.position, Quaternion.identity);
                InstanceObject.transform.position = spawnableObject.tr.transform.position;
            }

            if (!IsLoop)
                break;
        }
    }
}
