using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Cinemachine;

public class LevelManager : MonoBehaviour
{
    [Header("Entry Character")]
    public GameObject playerCharacter;

    [Header("Spawn")]
    public float respawnTime = 3f;

    [Header("GlobalLight")]
    [SerializeField] private Light2D globalLight;

    [HideInInspector] public CheckPoint currentCheckPoint;
    [HideInInspector] public ReturnPosition currentReturnPosition;
    [HideInInspector] public bool IsReturnPosition;
    [HideInInspector] public Room currentRoom;
    [HideInInspector] public float DefaultCameraShakeValue { get => defaultCameraShakeValue; }
    
    public enum FadeState { FadeIn, FadeOut, FadeInOut }
    private Coroutine coroutine;
    private Coroutine shakeCameraCoroutine;
    private Character character;
    private float shakeTimer = 0f;
    private float curShakeTime = 0f;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private Light2D playerLight;
    private float defaultCameraShakeValue = 0f;
    private static LevelManager instance = null;
    public static LevelManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        transform.parent = null;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        StartCoroutine(LevelStartSequance());
    }
    private IEnumerator LevelStartSequance()
    {
        playerCharacter = Instantiate(playerCharacter);
        playerCharacter.SetActive(false);
        character = playerCharacter.GetComponent<Character>();
        character.characterCondition = Character.CharacterCondition.Stunned;
        GUIManager.Instance.Blinder.color = Color.black;

        yield return new WaitForSeconds(respawnTime * 0.5f);
        FadeEvent(FadeState.FadeIn, respawnTime * 0.5f);
        playerCharacter.transform.position = currentCheckPoint.transform.position;
        playerCharacter.SetActive(true);
        currentRoom.FindPlayer(true);
        currentRoom.ChangeRoom();
        character.characterCondition = Character.CharacterCondition.Normal;
    }
    private void InitializedLevel()
    {
        currentCheckPoint.checkPointRoom.FindPlayer(true);
        currentCheckPoint.checkPointRoom.ChangeRoom();

        character.transform.rotation = Quaternion.identity;
        character.model.transform.rotation = Quaternion.identity;

        character.gameObject.SetActive(true);
        character.transform.position = currentCheckPoint.transform.position;
        character.IsSand = false;
        character.characterDig.StopAllCoroutines();
        character.characterWeaponAttack.StopAllCoroutines();

        character.characterDig.IsDigStop = false;
        character.velocity = Vector2.zero;
        character.characterCondition = Character.CharacterCondition.Stunned;

        IsReturnPosition = false;

        character.InitializedAnimation();
    }
    public void RespawnPlayer()
    {
        coroutine = StartCoroutine(RespawnDelay());
    }

    private IEnumerator RespawnDelay()
    {
        FadeEvent(FadeState.FadeOut, respawnTime);

        yield return new WaitForSeconds(respawnTime);
        RespawnObjects();
        InitializedLevel();

        yield return new WaitForSeconds(1f);
        FadeEvent(FadeState.FadeIn, respawnTime * 0.5f);

        yield return new WaitForSeconds(1f);
        character.characterCondition = Character.CharacterCondition.Normal;
    }
    public void RespawnObjects()
    {
        foreach (Room enemy in GameManager.Instance.rooms)
        {
            enemy.roomObject.RespawnPool();
        }
    }
    public void TimeEvent(float waitValue, float timeValue)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        Time.timeScale = timeValue;
        coroutine = StartCoroutine(TimeReset(waitValue));
    }

    private IEnumerator TimeReset(float waitValue)
    {
        yield return new WaitForSecondsRealtime(waitValue);
        Time.timeScale = 1f;
        //while (true)
        //{
        //    yield return new WaitForSecondsRealtime(0.02f);
        //    Time.timeScale += 0.02f;
        //    if (Time.timeScale >= 1f)
        //        break;
        //}
    }
    public void FadeEvent(FadeState fadeState, float duration)
    {
        switch (fadeState)
        {
            case FadeState.FadeOut:
                GUIManager.Instance.Blinder.color = new Color(0f, 0f, 0f, 0f);
                GUIManager.Instance.Blinder.DOColor(Color.black, duration);
                break;
            case FadeState.FadeIn:
                GUIManager.Instance.Blinder.color = Color.black;
                GUIManager.Instance.Blinder.DOColor(new Color(0f, 0f, 0f, 0f), duration);
                break;
            case FadeState.FadeInOut:
                coroutine = StartCoroutine(FadeInOutEvent(duration));
                break;
        }
    }
    private IEnumerator FadeInOutEvent(float duration)
    {
        FadeEvent(FadeState.FadeOut, duration);
        yield return new WaitForSecondsRealtime(1.0f);
        FadeEvent(FadeState.FadeIn, duration);
    }
    public void ShakeCameraEvent(float intensity, float time)
    {
        if (shakeTimer - curShakeTime >= time)
            return;

        if (shakeCameraCoroutine != null)
        {
            StopCoroutine(shakeCameraCoroutine);
        }

        cinemachineBasicMultiChannelPerlin = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.
            GetComponent<CinemachineVirtualCamera>().
            GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        shakeTimer = time;
        shakeCameraCoroutine = StartCoroutine(ShakeTimerCheck(intensity));
    }
    private IEnumerator ShakeTimerCheck(float intensity)
    {
        curShakeTime = 0f;
        var loop = new WaitForFixedUpdate();
        while (true)
        {
            yield return loop;
            curShakeTime += Time.deltaTime;

            if (cinemachineBasicMultiChannelPerlin.m_AmplitudeGain <= intensity)
            {
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
            }

            if (curShakeTime >= shakeTimer)
                break;
        }
        cinemachineBasicMultiChannelPerlin = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.
            GetComponent<CinemachineVirtualCamera>().
            GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = defaultCameraShakeValue;
    }
    public void SetShakeCameraValue(float shakeValue)
    {
        defaultCameraShakeValue = shakeValue;
        cinemachineBasicMultiChannelPerlin = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.VirtualCameraGameObject.
            GetComponent<CinemachineVirtualCamera>().
            GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = defaultCameraShakeValue;
    }
    public void ChangeLightSettingEvent(float globalIntensity, float innerRadius, float outerRadius)
    {
        if (playerLight == null)
        {
            playerLight = character.characterLight;
        }

        globalLight.intensity = globalIntensity;
        playerLight.pointLightInnerRadius = innerRadius;
        playerLight.pointLightOuterRadius = outerRadius;
    }
}
