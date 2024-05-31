using UnityEngine;
using DG.Tweening;
public class Photogene : MonoBehaviour
{
    [Header("Phtogene Component")]
    [SerializeField] private float fadeDuration = 1f;

    [HideInInspector] public bool IsClone = false;
    private SpriteRenderer sprite;
    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.sprite = LevelManager.Instance.playerCharacter.GetComponent<Character>().model.GetComponent<SpriteRenderer>().sprite;
        sprite.DOFade(0f, fadeDuration);
    }
}
