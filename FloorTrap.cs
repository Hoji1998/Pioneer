using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FloorTrap : MonoBehaviour
{
    [Header("FloorTrap Component")]
    [SerializeField] private List<GameObject> floors;
    [SerializeField] private GameObject breakFloorFeedback;
    [SerializeField] private GameObject shakeFloorFeedback;
    [SerializeField] private float shakeTime = 1.0f;
    private bool IsTrapOperationComplete;
    private void Start()
    {
        InitializedFloorTrap();
    }
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (floors.Count == 0)
            return;

        Gizmos.color = Color.yellow;
        foreach (GameObject floor in floors)
        {
            Gizmos.DrawLine(transform.position, floor.transform.position);
        }
#endif
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsTrapOperationComplete)
            return;

        Character character = collision.GetComponent<Character>();

        if (character == null)
            return;

        if (character.characterType != Character.CharacterType.Player)
            return;

        StartCoroutine(DestroyFloorsSequance());
    }
    private void InitializedFloorTrap()
    {
        IsTrapOperationComplete = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            floors.Add(transform.GetChild(i).gameObject);
        }
    }
    private void ShakeFloor()
    {
        foreach (GameObject floor in floors)
        {
            Instantiate(shakeFloorFeedback, floor.transform).transform.parent = null;
        }
    }
    private IEnumerator DestroyFloorsSequance()
    {
        IsTrapOperationComplete = true;
        //ShakeFloor();
        LevelManager.Instance.ShakeCameraEvent(5f, shakeTime);

        yield return new WaitForSeconds(shakeTime);

        foreach (GameObject floor in floors)
        {
            Instantiate(breakFloorFeedback, floor.transform).transform.parent = null;
            floor.SetActive(false);
        }
    }
}
