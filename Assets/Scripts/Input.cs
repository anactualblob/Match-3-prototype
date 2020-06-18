using UnityEngine;
using UnityEngine.InputSystem;

public class Input : MonoBehaviour
{

    DefaultInputActions defaultInputActions;



    public GameObject testPrefab;
    Camera mainCamera;


    [System.Flags]
    enum ButtonFlags : byte
    {
        none = 0b_0000_0000,                //default
        released = 0b_0000_0001,            //button is not being pressed
        releasedThisFrame = 0b_0000_0010,   //user stopped pressing the button this frame
        pressed = 0b_0000_0100,             //button is being pressed
        pressedThisFrame = 0b_0000_1000     //user started pressing the button this frame
    }

    ButtonFlags primaryTouch = 0;


    void Awake()
    {
        // Input Setup
        defaultInputActions = new DefaultInputActions();

        primaryTouch = ButtonFlags.released;


        // Object References
        mainCamera = Camera.main;
    }


    private void Update()
    {
        // mask primaryTouch with the inverse of the combination of pressedThisFRame and releasedThisFrame, 
        //  effectively unsetting pressedThisFrame and releasedThisFrame in primaryTouch.
        primaryTouch &= ~(ButtonFlags.pressedThisFrame | ButtonFlags.releasedThisFrame);

        switch (defaultInputActions.DefaultMap.Touch.phase)
        {
            case InputActionPhase.Started:
                byte pressedFlag = (byte)(primaryTouch & ButtonFlags.pressed);

                // this is a no branching version of only setting pressedThisFrame if pressed wasn't set. Basically
                //  we bitshift pressedFlag to be at the same spot as pressedThisFrame, and by AND'ing pressedThisFrame
                //  with the inverse of the bitshift result we only set pressedThisFrame if pressedFlag was 0.
                //  (the casting is because C# doesn't like bitwise operations on enums apparently)
                //  (and we also set pressed no matter what)
                primaryTouch |= (ButtonFlags)((byte)ButtonFlags.pressedThisFrame & ~(pressedFlag << 1)) | ButtonFlags.pressed;

                // we also unset released
                primaryTouch &= ~ButtonFlags.released;
                break;

            case InputActionPhase.Waiting:
                byte releasedFlag = (byte)(primaryTouch & ButtonFlags.released);

                // this is a no branching version of only setting releasedThisFrame if released wasn't set. Basically
                //  we bitshift releasedFlag to be at the same spot as releasedThisFrame, and by AND'ing releasedThisFrame
                //  with the inverse of the bitshift result we only set releasedThisFrame if releasedFlag was 0.
                //  (the casting is because C# doesn't like bitwise operations on enums apparently)
                //  (and we also set released no matter what)
                primaryTouch |= (ButtonFlags)((byte)ButtonFlags.releasedThisFrame & ~(releasedFlag << 1)) | ButtonFlags.released;

                // we also unset pressed
                primaryTouch &= ~ButtonFlags.pressed;
                break;

            default:
                break;
        }



        Vector2 touchPos = defaultInputActions.DefaultMap.TouchPosition.ReadValue<Vector2>();
        touchPos = mainCamera.ScreenToWorldPoint((Vector3)touchPos + Vector3.forward * 10);

        if ((primaryTouch & ButtonFlags.pressedThisFrame) == ButtonFlags.pressedThisFrame)
        {
            GridManager.TouchBegin( touchPos );
        }
        else if ((primaryTouch & ButtonFlags.pressed) == ButtonFlags.pressed)
        {
            GridManager.TouchMove(touchPos);
        }

        if ((primaryTouch & ButtonFlags.releasedThisFrame) == ButtonFlags.releasedThisFrame)
        {
            GridManager.TouchEnd(touchPos);
        } 
        else if ((primaryTouch & ButtonFlags.released) == ButtonFlags.released)
        {
            GridManager.NoTouch();
        }


    }




    private void OnEnable()
    {
        defaultInputActions.Enable();
    }

    private void OnDisable()
    {
        defaultInputActions.Disable(); 
    }
}
