using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractSwitch : MonoBehaviour
{
    [Header("InteractSwitch Component")]
    public InteractObject interactObject;
    public bool interactOn = false;
    public float limitTime = 3f;
    public int order = 0;
    public Animator animator;

    [HideInInspector] public Character character;

    private Coroutine coroutine;
    
    private void OnDrawGizmos()
    {
    #if UNITY_EDITOR
        if (interactObject == null)
            return;

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        style.fontSize = 40;
        style.fontStyle = FontStyle.Bold;

        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position, order.ToString(), style);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, interactObject.transform.position);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(interactObject.transform.position, 1.5f);
    #endif
    }
    private void Start()
    {
        if (interactObject == null)
            return;

        interactObject.interactSwitches.Add(this);
    }
    public void OnSwitch()
    {
        animator.speed = 0f;
        coroutine = StartCoroutine(CheckChacterMovementState());

        if (interactOn || interactObject == null)
            return;

        interactOn = true;
        
        if (interactObject.currentOrder <= order)
        {
            interactObject.currentOrder = order;
        }
        else
        {
            interactObject.IsCorrectOrder = false;
        }

        interactObject.StartInteract();
        ReturnSwitchState();
    }
    private IEnumerator CheckChacterMovementState()
    {
        var waitTime = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitTime;
            if (character.characterMoveState != Character.CharacterMoveState.Hanging)
            {
                break;
            }
        }
        animator.speed = 1f;
    }
    public void ReturnSwitchState()
    {
        if (limitTime == 0)
            return;

        CancelInvoke();
        Invoke("StopSwitch", limitTime);
    }
    public void StopSwitch()
    {
        interactOn = false;
        interactObject.StopInteract();
    }
}
