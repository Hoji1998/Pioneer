using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayerSearchField : MonoBehaviour
{
    [Header("PlayerSearchField")]
    [SerializeField] private List<AiBrain> aiBrains;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;


        foreach (AiBrain brain in aiBrains)
        {
            brain.IsSearchCharacter = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Character character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;


        foreach (AiBrain brain in aiBrains)
        {
            brain.IsSearchCharacter = false;
        }
    }
}
