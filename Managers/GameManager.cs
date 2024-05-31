using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Room> rooms;
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        Initialized();
        DontDestroyOnLoad(this.gameObject);
    }

    public void Initialized()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Room");
        foreach (GameObject room in gameObjects) 
        {
            rooms.Add(room.GetComponent<Room>());
        }
        transform.parent = null;
    }
}
