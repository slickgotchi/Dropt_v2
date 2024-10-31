using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class DroptCanvas : MonoBehaviour
{
    [Header("Hide/Show")]
    [SerializeField] private GameObject m_container;
    [SerializeField] private CanvasGroup m_canvasGroup;

    protected PlayerInput m_localPlayerInput;

    private void Awake()
    {
        HideCanvas();
    }

    protected virtual void Update()
    {
        TryGetLocalPlayerInput();
    }

    public void ShowCanvas()
    {
        m_container.SetActive(true);
        m_canvasGroup.blocksRaycasts = true;
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
        OnShowCanvas();
    }

    public void HideCanvas()
    {
        OnHideCanvas();
        m_container.SetActive(false);
        m_canvasGroup.blocksRaycasts = false;
        PlayerInputMapSwitcher.Instance.SwitchToInGame();
    }

    public bool IsActive()
    {
        return m_container.activeSelf;
    }

    public bool IsInputActionSelectPressed() {
        return (m_localPlayerInput != null
                && m_localPlayerInput.actions["Select"] != null
                && m_localPlayerInput.actions["Select"].IsPressed()
                && !PlayerInputMapSwitcher.Instance.IsSwitchTooRecent());
    }

    public virtual void OnShowCanvas() { }
    public virtual void OnHideCanvas() { }

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
