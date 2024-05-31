using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
[RequireComponent(typeof(CheckPoint))]
public class CheckPointChamber : ActionObject
{
    [Header("CheckPoinChamber Component")]
    [SerializeField] private KeyCode exitKeyCode;
    [SerializeField] private GameObject destinationPoint;
    [SerializeField] private float bufferTime = 0.6f;

    [Header("Chamber Fill")]
    [SerializeField] private Animator door;
    private CinemachineVirtualCamera virtualCamera;
    private CheckPoint checkPoint;
    private void Start()
    {
        checkPoint = GetComponent<CheckPoint>();
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }
    public override void ActionStart()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        EnterChamber();
    }

    private void EnterChamber()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        LevelManager.Instance.currentCheckPoint = checkPoint;
        LevelManager.Instance.RespawnObjects();
        GUIManager.Instance.CheckPointChamberCanvasVisible(true);

        virtualCamera.Priority = 11;

        character.health.InitializedHealth();

        character.animator.SetBool("Moving", true);
        character.FlipCharacter(destinationPoint.transform.position.x - character.transform.position.x);
        character.transform.DOMove(new Vector2(destinationPoint.transform.position.x, character.transform.position.y), 0.3f).OnComplete(EnterChamberComplete);
        StartCoroutine(ChamberEvent());
    }
    private void EnterChamberComplete()
    {
        door.SetBool("Close", true);
        character.animator.SetBool("Moving", false);
        character.characterMoveState = Character.CharacterMoveState.Hanging;
        character.model.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
    }
    private IEnumerator ChamberEvent()
    {
        yield return new WaitForSeconds(bufferTime);
        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(exitKeyCode))
            {
                ChamberClose();
                break;
            }
        }
    }
    private void ChamberClose()
    {
        character.InitializedAnimation();
        character.characterCondition = Character.CharacterCondition.Normal;
        character.characterAction.IsAction = false;
        character.gravityScale = character.initialGravityScale;

        virtualCamera.Priority = 0;
        GUIManager.Instance.CheckPointChamberCanvasVisible(false);
        character.model.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
        door.SetBool("Close", false);
    }
}
