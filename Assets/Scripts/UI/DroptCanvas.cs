using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using DG.Tweening;

public class DroptCanvas : MonoBehaviour
{
    [Header("Hide/Show")]
    [SerializeField] private GameObject m_container;
    [SerializeField] private CanvasGroup m_canvasGroup;

    protected PlayerInput m_localPlayerInput;

    public bool isCanvasOpen;

    private void Awake()
    {
        InstaHideCanvas();
    }

    protected virtual void Update()
    {
        TryGetLocalPlayerInput();
        OnUpdate();
    }

    public virtual void ShowCanvas()
    {
        m_canvasGroup.blocksRaycasts = true;
        m_canvasGroup.DOFade(1, 0.2f);
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
        OnShowCanvas();
        isCanvasOpen = true;
    }

    public void InstaHideCanvas()
    {
        m_canvasGroup.alpha = 0;
        PlayerInputMapSwitcher.Instance.SwitchToInGame();
        isCanvasOpen = false;
    }

    public virtual void HideCanvas()
    {
        OnHideCanvas();
        m_canvasGroup.blocksRaycasts = false;
        m_canvasGroup.DOFade(0, 0.2f);
        PlayerInputMapSwitcher.Instance.SwitchToInGame();
        isCanvasOpen = false;
    }

    public bool IsActive()
    {
        return m_container.activeSelf;
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
