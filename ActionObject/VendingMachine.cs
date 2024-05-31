using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : ActionObject
{
    [Header("VendingMachine Component")]
    [SerializeField] private KeyCode exitKeyCode;
    [SerializeField] private float bufferTime = 0.6f;
    public override void ActionStart()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        VendingMachineOperation();
    }
    private void VendingMachineOperation()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        character.velocity = Vector2.zero;
        character.animator.SetBool("Moving", false);
        GUIManager.Instance.VendingMachineCanvasVisible(true);
        StartCoroutine(VendingMachineEvent());
    }
    private IEnumerator VendingMachineEvent()
    {
        yield return new WaitForSeconds(bufferTime);
        while (true)
        {
            yield return null;

            if (Input.GetKeyDown(exitKeyCode))
            {
                Shutdown();
                break;
            }
        }
    }
    private void Shutdown()
    {
        character.InitializedAnimation();
        character.characterCondition = Character.CharacterCondition.Normal;
        character.characterAction.IsAction = false;
        GUIManager.Instance.VendingMachineCanvasVisible(false);
    }
}
