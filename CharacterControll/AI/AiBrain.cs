using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AiBrain : MonoBehaviour
{
    [Header("Brain")]
    public AiState currentAiState;
    public bool IsEngaged = false;
    public bool usePlayerSearchField = false;
    public bool useBrainWeight = false;

    [SerializeField] private float engageDistance = 5f;
    [SerializeField] private Brain[] brains;
    [SerializeField] private AiAbility IntroAbility;
    public enum AiState { Normal, Sleep, Proceeding }
    public int currentAiAbility = 0;

    [HideInInspector] public bool IsSearchCharacter = false;
    [HideInInspector] public Vector3 initializeTr;

    private Coroutine coroutine;
    private Character character;
    private Character playerCharacter;
    [Serializable] private struct Brain
    {
        public AiAbility ability;
        public float delayTime;
        public float duration;
        public int weight;
        public int weightIncrements;

        [HideInInspector] public int currentWeight;
    }
    private void Start()
    {
        character = GetComponent<Character>();
        for (int i = 0; i < brains.Length; i++)
        {
            brains[i].currentWeight = brains[i].weight;
        }
    }
    public void InitializedAi()
    {
        StopAllCoroutines();
        brains[currentAiAbility].ability.StopAllCoroutines();
        currentAiAbility = 0;
        currentAiState = AiState.Normal;
        IsSearchCharacter = false;
    }
    public void StopTotalAbility()
    {
        foreach (Brain brain in brains)
        {
            brain.ability.StopAllCoroutines();
        }
        InitializedAi();
    }
    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        switch (IsEngaged)
        {
            case true:
                Gizmos.color = Color.red;
                break;
            case false:
                Gizmos.color = Color.green;
                break;
        }
        Gizmos.DrawWireSphere(transform.position, engageDistance);
#endif
    }
    private void OnDisable()
    {
        InitializedAi();
        if (character == null)
            return;

        character.health.InitializedHealth();
        character.gravityScale = character.initialGravityScale;
        character.model.transform.rotation = Quaternion.identity;
    }
    private void OnEnable()
    {
        if (initializeTr == Vector3.zero)
        {
            initializeTr = transform.position;
        }
        transform.rotation = Quaternion.identity;
        transform.position = initializeTr;

        if (IntroAbility != null)
        {
            coroutine = StartCoroutine(IntroDelaySequence());
            return;
        }
        StartBrain();
    }
    public void AbilityChange()
    {
        CalculateBrainPriority();

        if (currentAiAbility >= brains.Length)
        {
            currentAiAbility = 0;
        }
        StopCoroutine(coroutine);
        coroutine = StartCoroutine(DelaySequence());
    }
    public void StartBrain()
    {
        coroutine = StartCoroutine(DelaySequence());
    }
    private void ProcessAbility()
    {
        if (playerCharacter == null)
        {
            playerCharacter = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }
        
        IsEngaged = Vector2.Distance(transform.position, playerCharacter.transform.position) <= engageDistance ? true : false;
        StartAi();
    }

    private bool AuthorizedAbility()
    {
        if (currentAiState != AiState.Normal)
        {
            return false;
        }

        return true;
    }

    private void StartAi()
    {
        brains[currentAiAbility].ability.ProcessAbility();
    }
    private IEnumerator IntroDelaySequence()
    {
        IntroAbility.InitializedAbility();
        currentAiState = AiState.Normal;

        yield return new WaitForSeconds(0.03f);
        IntroAbility.AbilityStart();
    }
    private IEnumerator DelaySequence()
    {
        brains[currentAiAbility].ability.InitializedAbility();
        currentAiState = AiState.Normal;

        yield return new WaitForSeconds(brains[currentAiAbility].delayTime);
        coroutine = StartCoroutine(ActiveSequence());
    }
    private IEnumerator ActiveSequence()
    {
        float curTime = 0f;
        while (true)
        {
            yield return null;
            curTime += Time.deltaTime;
            ProcessAbility();

            if (brains[currentAiAbility].duration == 0)
                continue;

            if (curTime >= brains[currentAiAbility].duration) 
            {
                break;
            }
        }

        AbilityChange();
    }
    private void CalculateBrainPriority()
    {
        if (!useBrainWeight)
        {
            currentAiAbility++;
            return; 
        }

        int currentWeight = 0;
        int totalWeight = 0;

        //total calculate
        for (int i = 0; i < brains.Length; i++)
        {
            totalWeight += brains[i].currentWeight;
        }

        //make random seed
        int selectNum = Mathf.RoundToInt(totalWeight * UnityEngine.Random.Range(0.0f, 1.0f));

        //select random seed
        for (int i = 0; i < brains.Length; i++)
        {
            currentWeight += brains[i].currentWeight;
            if (selectNum <= currentWeight)
            {
                currentAiAbility = i;
                return;
            }
        }
    }
    public void UpdateBrainWeight()
    {
        //update weight
        for (int i = 0; i < brains.Length; i++)
        {
            if (currentAiAbility == i)
            {
                brains[i].currentWeight = brains[i].weight;
                continue;
            }

            brains[i].currentWeight += brains[i].weightIncrements;
        }
    }
}
