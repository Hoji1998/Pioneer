using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractSwitch))]
public class Hanger : FinishAttackPoint
{
    private Character character;
    private InteractSwitch interactSwitch;

    public override void Start()
    {
        base.Start();
        interactSwitch = GetComponent<InteractSwitch>();
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    public override void FinishObjectFeedback()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        base.StartHitFeedback();
        character.characterMoveState = Character.CharacterMoveState.Hanging;
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        interactSwitch.character = character;
        interactSwitch.OnSwitch();
    }
}
