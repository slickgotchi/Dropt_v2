using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugLogDisplay : MonoBehaviour
{
    public static DebugLogDisplay Instance { get; private set; }

    [SerializeField] public GameObject container;
    [SerializeField] private TextMeshProUGUI logText; // Reference to the TextMeshProUGUI component
    [SerializeField] private ScrollRect scrollRect; // Reference to the Scroll Rect component

    private bool needsScrollUpdate = false; // Flag to indicate if the scroll needs updating

    private void Awake()
    {
        Instance = this;

        // Subscribe to the Application log message callback
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the callback to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha9))
        //{
        //    container.SetActive(!container.activeSelf);
        //}

        // Update the scroll position at the end of the frame if needed
        if (needsScrollUpdate)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            needsScrollUpdate = false; // Reset the flag
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Append the log message to the TextMeshProUGUI
        logText.text += $"{logString}\n";

        // Set the flag to update the scroll in the next frame
        needsScrollUpdate = true;
    }
}
