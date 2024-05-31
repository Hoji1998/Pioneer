using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TreasureChest : ActionObject
{
    [Header("TreasureBox Component")]
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject dropItem;
    [SerializeField] private string changeAnimationName;

    private bool IsUnlock = false;
    private Animator animator;
    private void Start()
    {
        animator = model.GetComponent<Animator>();
    }
    private void OnEnable()
    {
        if (IsUnlock)
            animator.SetBool(changeAnimationName, true);
    }
    public override void ActionStart()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        character.characterAction.IsAction = false;
        gameObject.layer = 0;

        IsUnlock = true;
        animator.SetBool(changeAnimationName, true);

        Invoke("DropItem", 1.33f);
    }
    public void DropItem()
    {
        Instantiate(dropItem, transform.position, Quaternion.identity);
    }
}
