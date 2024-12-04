using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class DebugConsole : MonoBehaviour, IDragHandler
{
    public KeyCode toggleKey = KeyCode.Alpha0;
    public int maxLogLength = 5000;
    public float defaultWidthPercent = 0.5f; // 50% of screen width
    public float defaultHeightPercent = 0.5f; // 50% of screen height
    public bool startVisible = false; // Indicates if the console should be visible at startup
    public Color titleColor = Color.cyan; // Color of the title text
    public Color normalColor = Color.white; // Color of the normal log text
    public Color warningColor = Color.yellow; // Color of the warning log text
    public Color errorColor = Color.red; // Color of the error/exception log text

    private static DebugConsole Instance;

    private GameObject consoleUI;
    private Text consoleText;
    private ScrollRect scrollRect;
    private RectTransform consoleRect;
    private RectTransform resizeHandle;
    private Text titleText;

    private bool isVisible;
    private StringBuilder logText = new StringBuilder();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isVisible = startVisible;
        CreateConsoleUI();
        Application.logMessageReceived += HandleLog;
    }

    private void Start()
    {
        // Generate test log messages
        for (int i = 0; i < 20; i++)
        {
            Debug.Log(i);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleConsole();
        }
    }

    private void CreateConsoleUI()
    {
        // Create console UI container
        consoleUI = new GameObject("DebugConsole");
        Canvas canvas = consoleUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler canvasScaler = consoleUI.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        GraphicRaycaster raycaster = consoleUI.AddComponent<GraphicRaycaster>();
        raycaster.enabled = true;

        // Create background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(consoleUI.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Partially transparent
        consoleRect = panel.GetComponent<RectTransform>();
        consoleRect.anchorMin = new Vector2(0, 0);
        consoleRect.anchorMax = new Vector2(0, 0);
        consoleRect.pivot = new Vector2(0, 0);
        consoleRect.sizeDelta = new Vector2(Screen.width * defaultWidthPercent, Screen.height * defaultHeightPercent);
        consoleRect.anchoredPosition = Vector2.zero;

        // Create title text
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(panel.transform);
        titleText = titleObject.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 16;
        titleText.color = titleColor;
        titleText.text = $"Console Log ({toggleKey.ToString()} to Toggle)";
        titleText.alignment = TextAnchor.MiddleLeft;
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0, 1);
        titleRect.offsetMin = new Vector2(10, -30);
        titleRect.offsetMax = new Vector2(0, 0);

        // Create ScrollRect
        GameObject scrollObject = new GameObject("ScrollRect");
        scrollObject.transform.SetParent(panel.transform);
        scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.offsetMin = new Vector2(10, 10);
        scrollRectTransform.offsetMax = new Vector2(-30, -40); // Adjusted for title height

        // Create Text element
        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(scrollObject.transform);
        consoleText = textObject.AddComponent<Text>();
        consoleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        consoleText.fontSize = 14;
        consoleText.color = normalColor;
        consoleText.raycastTarget = false;
        consoleText.alignment = TextAnchor.UpperLeft;
        consoleText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(0, 0);
        textRect.offsetMax = new Vector2(0, 0);

        // Create Scrollbar
        GameObject scrollbarObject = new GameObject("Scrollbar");
        scrollbarObject.transform.SetParent(panel.transform);
        Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.offsetMin = new Vector2(-20, 10);
        scrollbarRect.offsetMax = new Vector2(0, -40);

        // Link ScrollRect and Scrollbar
        scrollRect.content = textRect;
        scrollRect.verticalScrollbar = scrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // Create resize handle
        GameObject handle = new GameObject("ResizeHandle");
        handle.transform.SetParent(panel.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1, 1, 1, 0.5f); // Partially transparent
        handleImage.raycastTarget = true; // Allow dragging
        resizeHandle = handle.GetComponent<RectTransform>();
        resizeHandle.anchorMin = new Vector2(1, 1);
        resizeHandle.anchorMax = new Vector2(1, 1);
        resizeHandle.pivot = new Vector2(1, 1);
        resizeHandle.sizeDelta = new Vector2(20, 20);
        resizeHandle.anchoredPosition = Vector2.zero;

        // Add Event Trigger to handle dragging
        EventTrigger trigger = handle.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        entry.callback.AddListener((data) => { OnDrag((PointerEventData)data); });
        trigger.triggers.Add(entry);

        // Set the initial visibility state
        consoleUI.SetActive(isVisible);
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] {logString}\n";
        Color logColor;

        switch (type)
        {
            case LogType.Warning:
                logColor = warningColor;
                break;
            case LogType.Error:
            case LogType.Exception:
                logColor = errorColor;
                break;
            default:
                logColor = normalColor;
                break;
        }

        logText.Append($"<color=#{ColorUtility.ToHtmlStringRGB(logColor)}>{logEntry}</color>");

        if (logText.Length > maxLogLength)
        {
            logText.Remove(0, logText.Length - maxLogLength);
        }

        consoleText.text = logText.ToString();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ToggleConsole()
    {
        isVisible = !isVisible;
        consoleUI.SetActive(isVisible);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(consoleRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            consoleRect.sizeDelta = new Vector2(Mathf.Max(localPoint.x, 100), Mathf.Max(localPoint.y, 100)); // Minimum size of 100x100
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
