using UnityEngine;
using TMPro; // If you're using TextMeshPro

public class WebGLInputField : MonoBehaviour
{
    public TMP_InputField inputField; // Reference to your InputField (or InputField if not using TextMeshPro)

    void Update()
    {
        // Check for Cmd+V (Mac) or Ctrl+V (Windows) and paste content
        if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                string clipboardText = GUIUtility.systemCopyBuffer; // Get clipboard content
                if (inputField != null)
                {
                    inputField.text = clipboardText; // Paste clipboard content into the input field
                }
            }
        }
    }
}
