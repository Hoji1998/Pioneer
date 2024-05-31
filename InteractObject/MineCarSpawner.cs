using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineCarSpawner : MonoBehaviour
{
    [Header("MineCarSpawner Componenet")]
    [SerializeField] private MineCar mineCar;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private MineCar.MoveDirection moveDirection;

    [Header("Feedbacks")]
    [SerializeField] private float spawnTime = 0.5f;
    [SerializeField] private GameObject spawnFeedback;

    private Coroutine coroutine;
    private bool IsSpawn = false;
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (spawnPoint == null)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnPoint.transform.position, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, spawnPoint.transform.position);
#endif
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsSpawn)
            return;

        if (collision.gameObject.layer == 14)
        {
            return;
        }

        Weapon weapon = collision.GetComponent<Weapon>() == null ? collision.GetComponentInParent<Weapon>() : collision.GetComponent<Weapon>();

        if (weapon == null)
        {
            return;
        }
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        MineCarSpawn();
    }
    public void MineCarSpawn()
    {
        Instantiate(spawnFeedback, transform.position, Quaternion.identity);
        mineCar.DestroyCar();
        IsSpawn = true;

        coroutine = StartCoroutine(SpawnBuffer());
    }
    private IEnumerator SpawnBuffer()
    {
        var loop = new WaitForFixedUpdate();
        float curTime = 0f;

        while (curTime <= spawnTime)
        {
            yield return loop;
            curTime += Time.deltaTime;

            //2024. 02. 09. mine car spawn event 
        }

        SpawnComplete();
    }
    private void SpawnComplete()
    {
        mineCar.CreateCar(spawnPoint.transform, moveDirection);
        IsSpawn = false;
    }
}
