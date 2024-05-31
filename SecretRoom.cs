using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DG.Tweening;
public class SecretRoom : MonoBehaviour
{
    [Header("Connect Tile")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private BoxCollider2D tileFoundColl;
    BoundsInt area;
    Color tileColor;
    void Start()
    {
        tileColor = new Color(1f, 1f, 1f, 1f);
        Vector3Int position =Vector3Int.FloorToInt(tileFoundColl.bounds.min * 2);
        Vector3Int size = Vector3Int.FloorToInt(tileFoundColl.bounds.size * 2 + new Vector3Int(0, 0, 1));
        area = new BoundsInt(position, size);

        RoomStateChange(true);
        tilemap.color = Color.white;
    }
    public void RoomStateChange(bool value)
    {
        switch (value)
        {
            case true:
                foreach (Vector3Int point in area.allPositionsWithin)
                {
                    tilemap.SetTileFlags(point, TileFlags.None);
                    tilemap.SetColor(point, Color.white);
                }
                break;
            case false:
                StartCoroutine(TileFade());
                break;
        }
       
    }
    private IEnumerator TileFade()
    {
        tileColor = new Color(1f, 1f, 1f, 1f);

        while (true)
        {
            yield return new WaitForFixedUpdate();
            tileColor.a -= 0.03f;

            foreach (Vector3Int point in area.allPositionsWithin)
            {
                tilemap.SetColor(point, tileColor);
            }

            if (tileColor.a <= 0f)
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tileColor.a <= 0f)
            return;

        Character character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;

        RoomStateChange(false);
    }
}
