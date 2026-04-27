using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class InputHandler : MonoBehaviour
{
    public bool GetTapDown()
    {
#if ENABLE_INPUT_SYSTEM
        bool mouseTap = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        bool touchTap = false;
        if (Touchscreen.current != null)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            touchTap = primaryTouch.press.wasPressedThisFrame;
        }

        return mouseTap || touchTap;
#else
        bool mouseTap = Input.GetMouseButtonDown(0);

        bool touchTap = false;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchTap = touch.phase == TouchPhase.Began;
        }

        return mouseTap || touchTap;
#endif
    }
}
