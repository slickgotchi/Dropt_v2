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

        OnStart();
    }

    private void OnDestroy()
    {
        DOTween.Kill(m_modalRectTransform);
    }

    protected virtual void Update()
    {
        TryGetLocalPlayerInput();
        OnUpdate();

        // some backup keys for hiding the canvas
        if (Input.GetKeyDown(KeyCode.Return))
        {
            InstaHideCanvas();
        }
    }

    public virtual void ShowCanvas()
    {
        if (m_container == null || m_modal == null)
        {
            Debug.LogWarning("ShowCanvas: m_container or m_modal = null");
            return;
        }

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

        if (m_modalRectTransform != null)
        {
            m_modalRectTransform.DOAnchorPosX(m_modalOnscreenXPosition, 0.2f)
                .OnComplete(() => OnShowCanvas());
        }

        //Debug.Log("SwitchToInUI()");
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
        isCanvasOpen = true;
    }

    public void InstaHideCanvas()
    {
        if (m_container == null || m_modal == null)
        {
            Debug.LogWarning("ShowCanvas: m_container or m_modal = null");
            return;
        }

        if (m_modalRectTransform == null)
        {
            m_modalRectTransform = m_modal.GetComponent<RectTransform>();
        }

        if (m_backgroundCanvasGroup != null)
        {
            m_backgroundCanvasGroup.blocksRaycasts = false;
            m_backgroundCanvasGroup.alpha = 0;
        }

        if (m_modalRectTransform != null)
        {
            m_modalRectTransform.DOAnchorPosX(m_modalOffscreenXPosition, 0.2f)
                .OnComplete(() => m_container.SetActive(false));
        }


        // Immediately deactivate the container
        m_container.SetActive(false);

        //Debug.Log("SwitchToInGame()");
        PlayerInputMapSwitcher.Instance.SwitchToInGame();
        isCanvasOpen = false;
    }

    public virtual void HideCanvas()
    {
        if (m_container == null || m_modal == null)
        {
            Debug.LogWarning("ShowCanvas: m_container or m_modal = null");
            return;
        }

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

        if (m_modalRectTransform != null)
        {
            m_modalRectTransform.DOAnchorPosX(m_modalOffscreenXPosition, 0.2f)
                .OnComplete(() =>
                {
                    m_container.SetActive(false);
                });
        }

        //Debug.Log("SwitchToInGame()");
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
    public virtual void OnStart() { }
    public virtual void OnUpdate() { }

    protected void TryGetLocalPlayerInput()
    {
        if (m_localPlayerInput != null) return;

        var playerControllers = Game.Instance.playerControllers;
        foreach (var playerController in playerControllers)
        {
            if (playerController.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                m_localPlayerInput = playerController.GetComponent<PlayerInput>();
            }
        }
    }
}
