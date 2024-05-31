using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
    [Header("HealthBar Component")]
    [SerializeField] private Image healthBarBG;
    [SerializeField] private Image healthBar;

    [Header("Effect")]
    [SerializeField] private float duration = 0.5f;

    public void UpdateHealthBar(float value)
    {
        healthBar.color = new Color(1f, 1f, 1f, 0f);
        healthBar.DOKill();
        healthBar.DOFillAmount(value, duration).SetEase(Ease.OutExpo);

        if (value <= 0.3f)
        {
            healthBar.DOFade(1f, duration).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            healthBar.DOFade(1f, duration);
        }
    }
}
