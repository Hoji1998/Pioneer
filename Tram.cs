using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor;

public class Tram : ActionObject
{
    [Header("TramComponent")]
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject startValue;
    [SerializeField] private GameObject endValue;
    [SerializeField] private float duration = 2f;
    [SerializeField] private Animator door;
    [SerializeField] private GameObject destinationPoint;

    [Header("Tram GUI")]
    [SerializeField] private KeyCode exitKeyCode;
    [SerializeField] private float bufferTime = 0.6f;
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startValue.transform.position, endValue.transform.position);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endValue.transform.position, 0.5f);
#endif
    }
    public override void ActionStart()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        RideTheTram();
    }
    private void RideTheTram()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        GUIManager.Instance.CheckPointChamberCanvasVisible(true);

        character.animator.SetBool("Moving", true);
        character.FlipCharacter(destinationPoint.transform.position.x - character.transform.position.x);
        character.transform.DOMove(new Vector2(destinationPoint.transform.position.x, character.transform.position.y), 0.3f).OnComplete(EnterTramComplete);
        StartCoroutine(TramEvent());
    }
    private void EnterTramComplete()
    {
        DoorClose();
        character.animator.SetBool("Moving", false);
        character.characterMoveState = Character.CharacterMoveState.Hanging;
        character.model.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
    }
    private IEnumerator TramEvent()
    {
        yield return new WaitForSeconds(bufferTime);
        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(exitKeyCode))
            {
                TramExit();
                break;
            }
        }
    }
    private void TramExit()
    {
        character.InitializedAnimation();
        character.characterCondition = Character.CharacterCondition.Normal;
        character.characterAction.IsAction = false;
        character.gravityScale = character.initialGravityScale;

        GUIManager.Instance.CheckPointChamberCanvasVisible(false);
        character.model.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
        DoorOpen();
    }

    private void OnEnable()
    {
        body.transform.position = startValue.transform.position;
        DoorClose();
        TramStart();
    }
    public void TramStart()
    {
        body.transform.DOMove(endValue.transform.position, duration).OnComplete(DoorOpen);
    }

    private void DoorOpen()
    {
        door.SetBool("Close", false);
    }
    private void DoorClose()
    {
        door.SetBool("Close", true);
    }
}
