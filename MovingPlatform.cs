using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("MovingPlatform Component")]
    [SerializeField] private GameObject parentObject;
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!parentObject.activeSelf)
            return;

        transform.position = parentObject.transform.position;
    }
}
