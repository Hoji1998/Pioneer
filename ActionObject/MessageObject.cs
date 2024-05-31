using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageObject: ActionObject
{
    [Header("MessageObject Component")]
    [SerializeField] private KeyCode exitKeyCode;
    [SerializeField] private TextData message;
    [SerializeField] private float bufferTime = 0.6f;
    public override void ActionStart()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        MessageOpen();
    }
    private void MessageOpen()
    {
        character.characterCondition = Character.CharacterCondition.Stunned;
        character.velocity = Vector2.zero;
        character.animator.SetBool("Moving", false);
        GUIManager.Instance.MessageCanvasVisible(true, message.text);
        StartCoroutine(MessageEvent());
    }
    private IEnumerator MessageEvent()
    {
        yield return new WaitForSeconds(bufferTime);
        while (true)
        {
            yield return null;

            if (Input.GetKeyDown(exitKeyCode))
            {
                MessageClose();
                break;
            }
        }
    }
    private void MessageClose()
    {
        character.InitializedAnimation();
        character.characterCondition = Character.CharacterCondition.Normal;
        character.characterAction.IsAction = false;
        GUIManager.Instance.MessageCanvasVisible(false, message.text);
    }
}
