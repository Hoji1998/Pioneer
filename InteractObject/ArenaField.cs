using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class ArenaField : MonoBehaviour
{
    [Header("ArenaField Component")]
    [SerializeField] private List<InteractObject> interactObjects;
    [SerializeField] private List<EnemyPool> enemyPools;

    [Header("BossRoom Component")]
    [SerializeField] private bool IsBossRoom = false;
    [SerializeField] private CinemachineVirtualCamera bossCamera;

    [Header("Default CameraShake Value")]
    [SerializeField] private float cameraShakeValue = 1f;

    private CinemachineVirtualCamera virtualCamera;
    private bool onArena = false;
    private bool isComplete = false;
    private Character character;
    private Coroutine coroutine;
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (interactObjects.Count == 0)
            return;

        foreach (InteractObject interactObject in interactObjects)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, interactObject.transform.position);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactObject.transform.position, 1.5f);
        }
        
#endif
    }
    private void OnEnable()
    {
        InitializeArena();
    }
    private void InitializeArena()
    {
        onArena = false;
        StopCoroutine(ArenaStart());

        foreach (EnemyPool pool in enemyPools)
        {
            pool.gameObject.SetActive(false);
        }

        isComplete = false;

        if (virtualCamera == null)
        {
            virtualCamera = transform.GetChild(0).GetComponent<CinemachineVirtualCamera>();
        }
        virtualCamera.Priority = 0;

        foreach (InteractObject interactObject in interactObjects)
        {
            interactObject.StartInteract();
        }
    }
    private IEnumerator ArenaStart()
    {
        if (IsBossRoom)
        {
            BossCutSceneEvent();
        }

        onArena = true;
        virtualCamera.Follow = LevelManager.Instance.playerCharacter.transform;
        virtualCamera.Priority = 11;
        LevelManager.Instance.SetShakeCameraValue(cameraShakeValue);
        enemyPools[0].gameObject.SetActive(true);

        foreach (EnemyPool pool in enemyPools)
        {
            pool.gameObject.SetActive(true);
            pool.RespawnPool();

            bool enemyAlive = true;

            while (true)
            {
                yield return null;

                if (character.health.currentHealth <= 0f)
                {
                    break;
                }
                if (!pool.gameObject.activeSelf)
                {
                    pool.gameObject.SetActive(true);
                }

                enemyAlive = false;

                foreach (GameObject enemy in pool.Enemies)
                {
                    if (enemy.activeSelf)
                    {
                        enemyAlive = true;
                        break;
                    }
                }

                if (!enemyAlive)
                    break;

            }
        }
        virtualCamera.Priority = 0;
        isComplete = true;
        LevelManager.Instance.SetShakeCameraValue(0f);

        foreach (InteractObject interactObject in interactObjects)
        {
            interactObject.StartInteract();
        }
    }
    private void BossCutSceneEvent()
    {
        bossCamera.Priority = 12;
        coroutine = StartCoroutine(ReturnCamera());
    }
    private IEnumerator ReturnCamera()
    {
        yield return new WaitForSeconds(2f);
        bossCamera.Priority = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isComplete)
            return;

        if (onArena)
            return;

        character = collision.gameObject.GetComponent<Character>();

        if (character == null)
            return;
        if (character.characterType != Character.CharacterType.Player)
            return;

        foreach (InteractObject interactObject in interactObjects)
        {
            interactObject.StopInteract();
        }
        StartCoroutine(ArenaStart());
    }
}
