using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class GUIManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private CanvasGroup HUD;
    [SerializeField] private HealthBar healthBar;

    [Header("CheckPointChamberCanvas")]
    [SerializeField] private CanvasGroup checkPointChamberCanvas;

    [Header("MessageCanvas")]
    [SerializeField] private CanvasGroup messageCanvas;
    [SerializeField] private Text message;

    [Header("VendingMachineCanvas")]
    [SerializeField] private CanvasGroup vendingMachineCanvas;

    [Header("Blinder")]
    [SerializeField] private Image blinder;
    public Image Blinder { get => blinder; set => blinder = value; }

    private static GUIManager instance = null;
    private Character character;
    private PostProcessingControl postProcessingControl;
    public static GUIManager Instance
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
        Initialized();

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Initialized()
    {
        postProcessingControl = Camera.main.GetComponent<PostProcessingControl>();
    }
    public void UpdateHealth()
    {
        if (character == null)
        {
            character = LevelManager.Instance.playerCharacter.GetComponent<Character>();
        }

        float nomalizeValue = character.health.currentHealth / character.health.initialHealth;

        healthBar.UpdateHealthBar(nomalizeValue);
        postProcessingControl.PlayerHitVignetteEffect(nomalizeValue);
    }
    public void CheckPointChamberCanvasVisible(bool value)
    {
        switch (value)
        {
            case true:
                checkPointChamberCanvas.DOFade(1f, 0.5f);
                break;
            case false:
                checkPointChamberCanvas.DOFade(0f, 0.5f);
                break;
        }
    }
    public void MessageCanvasVisible(bool value, string messageValue)
    {
        message.text = messageValue;

        switch (value)
        {
            case true:
                messageCanvas.DOFade(1f, 0.5f);
                break;
            case false:
                messageCanvas.DOFade(0f, 0.5f);
                break;
        }
    }
    public void VendingMachineCanvasVisible(bool value)
    {
        switch (value)
        {
            case true:
                vendingMachineCanvas.DOFade(1f, 0.5f);
                break;
            case false:
                vendingMachineCanvas.DOFade(0f, 0.5f);
                break;
        }
    }
}
