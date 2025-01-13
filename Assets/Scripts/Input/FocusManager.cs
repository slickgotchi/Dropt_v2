using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// FocusManager
// - this manager is used to try stop the key lock issue from happening
// - e.g. when moving and right click some context, the player keeps moving and gets stuck

public class FocusManager : MonoBehaviour
{
    private bool hasFocus = true;

    // Called from JavaScript when the browser focus changes
    public void OnBrowserFocusChanged(int focused)
    {
        hasFocus = (focused == 1);

        if (hasFocus)
        {
            Debug.Log("Browser regained focus.");
            ResetInput();
        }
        else
        {
            Debug.Log("Browser lost focus.");
        }
    }

    private void ResetInput()
    {
        var keyboard = Keyboard.current;
        var keyboardState = new KeyboardState();
        keyboardState.Release(Key.W);
        keyboardState.Release(Key.A);
        keyboardState.Release(Key.S);
        keyboardState.Release(Key.D);
        InputSystem.QueueStateEvent(keyboard, keyboardState);
        Debug.Log("Release keyboard state");
    }

    private void Update()
    {
        if (!hasFocus)
        {
            // Block input or handle special cases while unfocused
        }
    }
}
