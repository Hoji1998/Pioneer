using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialDestructionObject : MonoBehaviour
{
    [Header("SpecialDestructionObject Component")]
    [SerializeField] private GameObject model;
    [SerializeField] private Character.CharacterMoveState destructionState;

    [Header("Feedbacks")]
    [SerializeField] private GameObject destroyFeedback;

    private void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        gameObject.SetActive(true);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;
        if (character.characterMoveState != destructionState)
            return;

        DestructionStart();
    }

    private void DestructionStart()
    {
        LevelManager.Instance.ShakeCameraEvent(5f, 0.2f);
        Instantiate(destroyFeedback, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
