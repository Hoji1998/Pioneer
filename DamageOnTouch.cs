using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [Header("Authorized ReturnPosition")]
    [SerializeField] private bool authorizedReturnPosition = false;
    [SerializeField] private float damage = 20f;
    private void OnTriggerStay2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;
        if (character.health.invulnerable)
            return;

        if (authorizedReturnPosition)
        {
            LevelManager.Instance.IsReturnPosition = true;
        }
        LevelManager.Instance.TimeEvent(0.2f, 0f);
        LevelManager.Instance.ShakeCameraEvent(5f, 0.1f);
        character.health.DamageCalculate(damage);
    }
}
