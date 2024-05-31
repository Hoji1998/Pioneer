using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiDestroyEvent : AiAbility
{
    [Header("Destroy Colliders")]
    [SerializeField] private List<GameObject> colls;

    private Coroutine coroutine;
    private void OnEnable()
    {
        foreach (GameObject coll in colls)
        {
            coll.SetActive(true);
        }
    }
    public override void AbilityStart()
    {
        brain.StopTotalAbility();
        LevelManager.Instance.playerCharacter.GetComponent<Character>().health.InitializedHealth();
        coroutine = StartCoroutine(DestroySequanceStart());
    }
    private IEnumerator DestroySequanceStart()
    {
        foreach (GameObject coll in colls)
        {
            coll.SetActive(false);
        }

        LevelManager.Instance.TimeEvent(0.3f, 0f);
        //LevelManager.Instance.SetShakeCameraValue(10f);
        LevelManager.Instance.ShakeCameraEvent(10f, 4.9f);
        AnimationStateChange(true);
        character.velocity = Vector2.zero;
        yield return new WaitForSeconds(5f);
        LevelManager.Instance.ShakeCameraEvent(15f, 0.3f);
        character.health.Destroy();
    }
}
