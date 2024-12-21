using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using DG.Tweening;

public class DroptCanvas : MonoBehaviour
{
    [Header("Hide/Show")]
    [SerializeField] private GameObject m_container;
    [SerializeField] private CanvasGroup m_backgroundCanvasGroup;
    [SerializeField] private GameObject m_modal;
    [SerializeField] private float m_modalOffscreenXPosition = -1000;
    [SerializeField] private float m_modalOnscreenXPosition = 0;

    private RectTransform m_modalRectTransform;

    protected PlayerInput m_localPlayerInput;

    public bool isCanvasOpen;

    private void Start()
    {
        if (m_backgroundCanvasGroup != null)
        {
            m_backgroundCanvasGroup.blocksRaycasts = false;
            m_backgroundCanvasGroup.alpha = 1;
        }

        InstaHideCanvas();
    }

    protected virtual void Update()
    {
        TryGetLocalPlayerInput();
        OnUpdate();
    }

    public virtual void ShowCanvas()
    {
        if (m_modalRectTransform == null)
        {
            m_modalRectTransform = m_modal.GetComponent<RectTransform>();
        }

        // Reactivate container at the start of the animation
        m_container.SetActive(true);

        if (m_backgroundCanvasGroup != null)
        {
            m_backgroundCanvasGroup.blocksRaycasts = true;
            m_backgroundCanvasGroup.DOFade(1, 0.2f);
        }

        m_modalRectTransform.DOAnchorPosX(m_modalOnscreenXPosition, 0.2f)
            .OnComplete(() => {
                OnShowCanvas(); // Custom logic after showing canvas
            });

        PlayerInputMapSwitcher.Instance.SwitchToInUI();
        isCanvasOpen = true;
    }

    public void InstaHideCanvas()
    {
        if (m_modalRectTransform == null)
        {
            m_modalRectTransform = m_modal.GetComponent<RectTransform>();
        }

        if (m_backgroundCanvasGroup != null)
        {
            m_backgroundCanvasGroup.blocksRaycasts = false;
            m_backgroundCanvasGroup.alpha = 0;
        }

        m_modalRectTransform.anchoredPosition =
            new Vector2(m_modalOffscreenXPosition, m_modalRectTransform.anchoredPosition.y);

        // Immediately deactivate the container
        m_container.SetActive(false);

        PlayerInputMapSwitcher.Instance.SwitchToInGame();
        isCanvasOpen = false;
    }

    public virtual void HideCanvas()
    {
        if (m_modalRectTransform == null)
        {
            m_modalRectTransform = m_modal.GetComponent<RectTransform>();
        }

        OnHideCanvas();
        if (m_backgroundCanvasGroup != null)
        {
            m_backgroundCanvasGroup.blocksRaycasts = false;
            m_backgroundCanvasGroup.DOFade(0, 0.2f);
        }

        m_modalRectTransform.DOAnchorPosX(m_modalOffscreenXPosition, 0.2f)
            .OnComplete(() => {
                // Deactivate the container after the animation completes
                m_container.SetActive(false);
            });

        PlayerInputMapSwitcher.Instance.SwitchToInGame();
        isCanvasOpen = false;
    }

    public bool IsCanvasOpen()
    {
        return isCanvasOpen;
    }

    public bool IsInputActionSelectPressed()
    {
        return (m_localPlayerInput != null
                && m_localPlayerInput.actions["Select"] != null
                && m_localPlayerInput.actions["Select"].IsPressed()
                && !PlayerInputMapSwitcher.Instance.IsSwitchTooRecent());
    }

    public virtual void OnShowCanvas() { }
    public virtual void OnHideCanvas() { }
    public virtual void OnUpdate() { }

    protected void TryGetLocalPlayerInput()
    {
        if (m_localPlayerInput != null) return;

        var playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var playerInput in playerInputs)
        {
            if (playerInput.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                m_localPlayerInput = playerInput.GetComponent<PlayerInput>();
            }
        }
    }
}
