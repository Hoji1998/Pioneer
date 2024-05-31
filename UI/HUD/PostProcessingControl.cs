using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
public class PostProcessingControl : MonoBehaviour
{
    private Volume volume;
    private Vignette vignette;
    private MotionBlur motionBlur;
    private Tween tween;
    private void Start()
    {
        volume = Camera.main.GetComponent<Volume>();
        volume.profile.TryGet<Vignette>(out vignette);
        volume.profile.TryGet<MotionBlur>(out motionBlur);
    }
    public void MotionBluerEffect(float value)
    {
        motionBlur.intensity.value = value;
    }
    public void PlayerHitVignetteEffect(float value)
    {
        vignette.intensity.value = 0.5f;

        if (value > 0.3f)
        {
            tween = DOTween.To(() => vignette.intensity.value, value => vignette.intensity.value = value, 0f, 0.5f).SetLoops(1, LoopType.Yoyo);
        }
    }
}
