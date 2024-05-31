using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [SerializeField] private List<GameObject> enemies;

    [HideInInspector] public List<GameObject> Enemies { get => enemies; }
    private AiBrain[] gameObjects;
    private ArenaField[] arenaFields;
    private void Start()
    {
        FindEnemyList();
    }
    public void FindEnemyList()
    {
        gameObjects = GetComponentsInChildren<AiBrain>();
        arenaFields = GetComponentsInChildren<ArenaField>();

        foreach (AiBrain enemy in gameObjects)
        {
            enemies.Add(enemy.gameObject);
        }
        foreach (ArenaField arenaField in arenaFields)
        {
            enemies.Add(arenaField.gameObject);
        }
        this.gameObject.SetActive(false);
    }
    public void RespawnPool()
    {
        if (gameObjects == null)
            return;

        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(false);
        }

        foreach (GameObject enemy in enemies)
        {
            enemy.SetActive(true);
        }
    }
}
