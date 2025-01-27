using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GotchiHub;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class EssenceBurnCanvas : DroptCanvas
{
    public static EssenceBurnCanvas Instance { get; private set; }

    [HideInInspector] public Interactable interactable;

    [SerializeField] private Button m_exitButton;

    private void Awake()
    {
        // Singleton pattern 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_exitButton.onClick.AddListener(HandleClickExit);

        InstaHideCanvas();

    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();

    }

    public override void OnHideCanvas()
    {
        base.OnHideCanvas();
    }

    public override void OnUpdate()
    {

    }

    void HandleClickExit()
    {
        EssenceBurnCanvas.Instance.HideCanvas();
        if (interactable != null)
        {
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactable.interactionText,
                interactable.interactableType);
        }
    }
}
