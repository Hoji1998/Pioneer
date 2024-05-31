using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Component")]
    [SerializeField] private Collider2D[] colls;
    public float preparingAttackBuffer = 0.5f;
    public float underAttackBuffer = 0.5f;
    public float attackDoneBuffer = 0f;
    public WeaponState currentWeaponState;

    [Header("Effect")]
    [SerializeField] private GameObject model;

    [HideInInspector] public GameObject Model { get => model; }
    [HideInInspector] public float comboAttackBuffer { get; set; }
    [HideInInspector] public float PreparingAttackBuffer { get => preparingAttackBuffer; }
    [HideInInspector] public float UnderAttackBuffer { get => underAttackBuffer; }
    [HideInInspector] public float AttackDoneBuffer { get => attackDoneBuffer; }

    [HideInInspector] public float weaponDamage = 1f;
    [HideInInspector] public CharacterWeaponAttack characterWeaponAttack;
    [HideInInspector] public int currentCombo = 0;
    [HideInInspector] public Coroutine coroutine;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Character character;
    public enum WeaponState { PreparingAttack, UnderAttack, AttackDone, Ready} 
    public virtual void Start()
    {
        anim = model.GetComponent<Animator>();
        comboAttackBuffer = 1f;
    }
    private void OnEnable()
    {
        InitializedAttackState();
    }
    public virtual void InitializedAttackState()
    {
        foreach (Collider2D coll in colls)
        {
            coll.enabled = false;
        }
        if (characterWeaponAttack != null)
        {
            characterWeaponAttack.AnimationStateChange(false);
        }
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        WeaponStateChange(WeaponState.Ready);
    }
    public virtual void OperateAttack()
    {
        PreparingAttackEvent();
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(PreparingAttack());
    }
    #region Weapon Coroutine
    public virtual IEnumerator PreparingAttack()
    {
        yield return new WaitForSeconds(preparingAttackBuffer);

        UnderAttackEvent();
        coroutine = StartCoroutine(UnderAttack());
    }

    public virtual IEnumerator UnderAttack()
    {
        yield return new WaitForSeconds(underAttackBuffer);
        AttackDoneEvent();
        coroutine = StartCoroutine(AttackDone());
    }

    public virtual IEnumerator AttackDone()
    {
        yield return new WaitForSeconds(attackDoneBuffer * comboAttackBuffer);

        InitializedAttackState();
    }
    #endregion

    #region AttackEvent
    public virtual void PreparingAttackEvent()
    {
        WeaponStateChange(WeaponState.PreparingAttack);
    }
    public virtual void UnderAttackEvent()
    {
        if (character.characterCondition == Character.CharacterCondition.Stunned)
        {
            return;
        }
        WeaponStateChange(WeaponState.UnderAttack);
        colls[currentCombo].enabled = true;
    }

    public virtual void AttackDoneEvent()
    {
        WeaponStateChange(WeaponState.AttackDone);
        foreach (Collider2D coll in colls)
        {
            coll.enabled = false;
        }
    }
    #endregion

    public virtual void WeaponStateChange(WeaponState weaponState)
    {
        currentWeaponState = weaponState;

        if (anim == null)
            return;

        switch (weaponState)
        {
            case WeaponState.PreparingAttack:
                anim.SetBool("Ready", false);
                anim.SetBool("PreparingAttack", true);
                anim.SetBool("UnderAttack", false);
                anim.SetBool("AttackDone", false);
                break;
            case WeaponState.UnderAttack:
                anim.SetBool("Ready", false);
                anim.SetBool("PreparingAttack", false);
                anim.SetBool("UnderAttack", true);
                anim.SetBool("AttackDone", false);
                break;
            case WeaponState.AttackDone:
                anim.SetBool("Ready", false);
                anim.SetBool("PreparingAttack", false);
                anim.SetBool("UnderAttack", false);
                anim.SetBool("AttackDone", true);
                break;
            case WeaponState.Ready:
                anim.SetBool("Ready", true);
                anim.SetBool("PreparingAttack", false);
                anim.SetBool("UnderAttack", false);
                anim.SetBool("AttackDone", false);                
                break;
        }
    }
}
