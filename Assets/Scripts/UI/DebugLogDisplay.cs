using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugLogDisplay : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI logText; // Reference to the TextMeshProUGUI component
    [SerializeField] private ScrollRect scrollRect; // Reference to the Scroll Rect component

    private void Awake()
    {
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
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            container.SetActive(!container.activeSelf);
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Append the log message to the TextMeshProUGUI
        logText.text += $"{logString}\n";

        // Scroll to the bottom
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
