using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("CheckPoint Component")]
    [SerializeField] private bool IsDebugPoint = false;
    [SerializeField] private bool collideCheck = true;
    [SerializeField] private LayerMask confinerLayer;

    [HideInInspector] public Room checkPointRoom;

    Collider2D hit;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    
    private void Start()
    {
        hit = Physics2D.OverlapPoint(transform.position, confinerLayer);
        if (hit != null)
        {
            checkPointRoom = hit.gameObject.GetComponentInParent<Room>();
        }

        if (IsDebugPoint)
        {
            LevelManager.Instance.currentRoom = checkPointRoom;
            LevelManager.Instance.currentCheckPoint = this;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collideCheck)
            return;

        Character character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;

        LevelManager.Instance.currentCheckPoint = this;
    }
}
