using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Teleport : ActionObject
{
    [Header("Teleport")]
    [SerializeField] private TeleportType teleportType;
    [SerializeField] private LayerMask confinerLayer;

    [Header("FadeEvent")]
    [SerializeField] private float fadeDuration = 0f;
    [SerializeField] private GameObject destinationPoint;
    public enum TeleportType { None, Fade }
    private Coroutine coroutine;
    private Room currentRoom;
    private Room destinationRoom;
    private Collider2D hit;
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, destinationPoint.transform.position);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(destinationPoint.transform.position, 0.5f);
#endif
    }
    private void Start()
    {
        hit = Physics2D.OverlapPoint(transform.position, confinerLayer);
        currentRoom = hit.gameObject.GetComponent<Room>();
        
        hit = Physics2D.OverlapPoint(destinationPoint.transform.position, confinerLayer);
        destinationRoom = hit.gameObject.GetComponentInParent<Room>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (base.authorizedAction)
            return;

        character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterAction == null)
            return;
        if (character.characterAction.IsAction)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;

        TeleportStart();
    }
    public override void ActionStart()
    {
        TeleportStart();
    }
    private void TeleportStart()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        character.gravityScale = 0f;
        character.characterAction.IsAction = true;

        switch (teleportType)
        {
            case TeleportType.None:
                character.gameObject.transform.DOMove(destinationPoint.transform.position, fadeDuration).OnComplete(destinationRoom.ChangeRoom);
                currentRoom.FindPlayer(false);
                destinationRoom.FindPlayer(true);
                break;
            case TeleportType.Fade:
                if (authorizedAction)
                {
                    character.animator.SetBool("Moving", true);
                    character.FlipCharacter(transform.position.x - character.transform.position.x);
                    character.transform.DOMove(new Vector2(transform.position.x, character.transform.position.y), 0.3f).OnComplete(EnterTeleportDestination);
                }
                else
                {
                    coroutine = StartCoroutine(WaitFadeEvent(fadeDuration * 0.5f));
                }
                break;
        }
    }
    private void EnterTeleportDestination()
    {
        character.animator.SetBool("Moving", false);
        character.characterMoveState = Character.CharacterMoveState.Hanging;

        coroutine = StartCoroutine(WaitFadeEvent(fadeDuration * 0.5f));
    }
    private IEnumerator WaitFadeEvent(float duration)
    {
        LevelManager.Instance.FadeEvent(LevelManager.FadeState.FadeOut, duration);

        yield return new WaitForSeconds(duration);
        Vector2 moveVec = character.velocity;
        character.gameObject.transform.position = destinationPoint.transform.position;
        destinationRoom.FindPlayer(true);
        destinationRoom.ChangeRoom();

        yield return new WaitForSeconds(duration);
        character.InitializedAnimation();
        character.characterCondition = Character.CharacterCondition.Normal;
        character.characterAction.IsAction = false;
        character.gravityScale = character.initialGravityScale;
        character.velocity = moveVec;

        LevelManager.Instance.FadeEvent(LevelManager.FadeState.FadeIn, duration);
        character.gameObject.transform.position = destinationPoint.transform.position;
        character.gravityScale = character.initialGravityScale;
    }
}
