using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CopyAnimationState : MonoBehaviour
{
    [Header("CopyAnimationState Component")]
    public List<ChildAnim> childAnims;
    [Serializable] public struct ChildAnim
    {
        public Animator animator;
        public string changeAnimationName;
    }

    public void ChildAnimationOn(int index)
    {
        childAnims[index].animator.SetBool(childAnims[index].changeAnimationName, true);
    }
    public void ChildAnimationOff(int index)
    {
        childAnims[index].animator.SetBool(childAnims[index].changeAnimationName, false);
    }
    public void AllAnimationOff()
    {
        foreach (ChildAnim childAnim in childAnims)
        {
            childAnim.animator.SetBool(childAnim.changeAnimationName, false);
        }
    }
}
