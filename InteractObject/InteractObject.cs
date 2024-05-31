using Cinemachine;
using Cinemachine.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractObject : MonoBehaviour
{
    [Header("Interact Component")]
    public List<InteractObject> additionalInteractObjects;
    public bool IsOpenInf = false;

    [HideInInspector] public bool authorizedInteract = true;
    [HideInInspector] public List<InteractSwitch> interactSwitches;
    [HideInInspector] public bool IsCorrectOrder = true;
    [HideInInspector] public int currentOrder = -1;
    [HideInInspector] public CinemachineVirtualCamera virtualCamera;
    public virtual void OnDrawGizmos()
    {
    #if UNITY_EDITOR
        if (additionalInteractObjects.Count == 0)
            return;

        foreach (InteractObject additionalInteractObject in additionalInteractObjects)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, additionalInteractObject.transform.position);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(additionalInteractObject.transform.position, 1.5f);
        }
    #endif
    }
    public virtual void Start()
    {
        virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
    }
    public virtual void ResetInteractObject()
    {
        foreach (InteractSwitch interactSwitch in interactSwitches)
        {
            interactSwitch.interactOn = false;
        }
    }
    public virtual void StartInteract()
    {
        if (!AuthorizedInteract())
            return;

        if (!CorrectOrderCheck())
            return;

        InteractEvent();
        foreach (InteractObject additionalInteractObject in additionalInteractObjects)
        {
            additionalInteractObject.InteractEvent();
        }
    }
    public virtual void InteractEvent()
    {
        
    }
    public virtual void StopInteract()
    {
        foreach (InteractObject additionalInteractObject in additionalInteractObjects)
        {
            additionalInteractObject.StopInteract();
        }
    }
    public virtual bool AuthorizedInteract()
    {
        if (!authorizedInteract)
            return false;

        foreach (InteractSwitch interactSwitch in interactSwitches)
        {
            if (!interactSwitch.interactOn)
            {
                return false;
            }
        }
        return true;
    }
    public virtual bool CorrectOrderCheck()
    {
        if (IsCorrectOrder)
            return true;

        foreach (InteractSwitch interactSwitch in interactSwitches)
        {
            interactSwitch.CancelInvoke();
            interactSwitch.StopSwitch();
        }
        currentOrder = -1;
        IsCorrectOrder = true;
        return false;
    }
    public virtual void ReturnCameraPriority()
    {
        virtualCamera.Priority = 0;
    }
    public virtual void OpenInfiniteCheck()
    {
        if (!IsOpenInf)
            return;

        foreach (InteractSwitch interactSwitch in interactSwitches)
        {
            interactSwitch.CancelInvoke();
            interactSwitch.limitTime = 0;
        }
        authorizedInteract = false;
    }
}
