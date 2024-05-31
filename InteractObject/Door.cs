using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Door : InteractObject
{
    [Header("Door Component")]
    [SerializeField] private bool IsOpen = false;

    [Header("Door Camera")]
    [SerializeField] private int cameraPriority = 11;
    [SerializeField] private bool authorizedDoorCamera = true;

    private SpriteRenderer sprite;
    private Collider2D coll;
    private Animator anim;

    public override void Start()
    {
        base.Start();
        sprite= GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (coll == null)
        {
            coll = GetComponent<Collider2D>();
        }

        switch (IsOpen)
        {
            case false:
                anim.SetFloat("Mover", 1.0f);
                anim.SetFloat("Reverse", 1.0f);
                coll.enabled = true;
                break;
            case true:
                anim.SetFloat("Mover", -1.0f);
                anim.SetFloat("Reverse", 1.0f);
                coll.enabled = false;
                break;
        }
    }
    public override void InteractEvent()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (coll == null)
        {
            coll = GetComponent<Collider2D>();
        }

        coll.enabled= false;
        IsOpen = true;

        if (authorizedDoorCamera)
        {
            virtualCamera.Priority = cameraPriority;
            Invoke("ReturnCameraPriority", 3f);
        }

        anim.SetFloat("Mover", -1.0f);
        anim.SetFloat("Reverse", 1.0f);
        

        OpenInfiniteCheck();
    }
    public override void StopInteract()
    {
        base.StopInteract();
        coll.enabled= true;
        IsOpen = false;

        anim.SetFloat("Mover", 1.0f);
        anim.SetFloat("Reverse", 1.0f);
    }
}
