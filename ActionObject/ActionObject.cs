using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public abstract class ActionObject : MonoBehaviour
{
    [Header("Action Component")]
    public bool authorizedAction = false;
    public SpriteRenderer actionMarker;

    [HideInInspector] public Character character;
    public virtual void ActionStart()
    {

    }

    public virtual void ActionMarkerUpdate(bool value)
    {
        if (!authorizedAction)
            return;

        switch (value)
        {
            case true:
                actionMarker.DOFade(1f, 0.5f);
                break;
            case false:
                actionMarker.DOFade(0f, 0.5f);
                break;
        }
    }
}
