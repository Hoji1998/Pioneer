using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Setting Buttons")]
    [SerializeField] private KeyCode leftKey;
    [SerializeField] private KeyCode rightKey;
    [SerializeField] private KeyCode upKey;
    [SerializeField] private KeyCode downKey;
    [SerializeField] private KeyCode jumpKey;

    [HideInInspector] public ButtonState leftButton;
    [HideInInspector] public ButtonState rightButton;
    [HideInInspector] public ButtonState upButton;
    [HideInInspector] public ButtonState downButton;
    [HideInInspector] public ButtonState jumpButton;
    [HideInInspector] public Vector2 movementValue = Vector2.zero;

    private List<ButtonState> buttons;
    public struct ButtonState
    {
        public void Reset()
        {
            ButtonDown = false;
            ButtonPressed = false;
            ButtonUp = false;
        }
        public void Down()
        {
            ButtonDown = true;
        }
        public void Pressed()
        {
            ButtonPressed = true;
        }
        public void Up()
        {
            ButtonUp = true;
        }
        public bool ButtonDown;
        public bool ButtonPressed;
        public bool ButtonUp;
        public KeyCode keyCode;
    }
    private static InputManager instance = null;
    public static InputManager Instance
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
        //DontDestroyOnLoad(this.gameObject);
    }
    private void Update()
    {
        ProcessInput();
        MovementCheck();
    }
    private void Initialized()
    {
        leftButton.keyCode = leftKey;
        rightButton.keyCode = rightKey;
        upButton.keyCode = upKey;
        downButton.keyCode = downKey;
        jumpButton.keyCode = jumpKey;

        buttons = new List<ButtonState>();
        buttons.Add(leftButton);
        buttons.Add(rightButton);
        buttons.Add(upButton);
        buttons.Add(downButton);
        buttons.Add(jumpButton);
    }
    private void ProcessInput()
    {

        ////jumpbutton.reset();
        ////if (input.getkeydown(jumpkey))
        ////{
        ////    jumpbutton.buttondown = true;
        ////}
        ////else if (input.getkey(jumpkey))
        ////{
        ////    jumpbutton.buttonpressed = true;
        ////}
        ////else if (input.getkeyup(jumpkey))
        ////{
        ////    jumpbutton.buttonup = true;
        ////}
    }
    private void MovementCheck()
    {
        if (leftButton.ButtonDown || leftButton.ButtonPressed)
        {
            movementValue.x = -1f;
        }

        if (rightButton.ButtonDown || rightButton.ButtonPressed)
        {
            movementValue.x = 1f;
        }

        if (!leftButton.ButtonDown && !leftButton.ButtonPressed
            && !rightButton.ButtonDown && !rightButton.ButtonPressed)
        {
            movementValue.x = 0f;
        }

        if (upButton.ButtonDown || upButton.ButtonPressed)
        {
            movementValue.y = 1f;
        }

        if (downButton.ButtonDown || downButton.ButtonPressed)
        {
            movementValue.y = -1f;
        }

        if (!upButton.ButtonDown && !upButton.ButtonPressed
            && !downButton.ButtonDown && !downButton.ButtonPressed)
        {
            movementValue.y = 0f;
        }
    }
}
