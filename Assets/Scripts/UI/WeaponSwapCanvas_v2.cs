using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GotchiHub;
using Unity.Netcode;
using UnityEngine.InputSystem;
using TMPro;
using DG.Tweening;

public class WeaponSwapCanvas_v2 : DroptCanvas
{
    public static WeaponSwapCanvas_v2 Instance { get; private set; }

    [SerializeField] private EquipmentSwapWeaponCard m_newWeapon;
    [SerializeField] private EquipmentSwapWeaponCard m_lhWeapon;
    [SerializeField] private EquipmentSwapWeaponCard m_rhWeapon;

    [SerializeField] private TextMeshProUGUI m_swapInfoText;

    [HideInInspector] public Wearable.NameEnum newWeaponNameEnum;
    [HideInInspector] public int gotchiId;
    private PlayerController m_localPlayer;

    [HideInInspector] public WeaponSwap activeWeaponSwap;

    public enum NewWeaponPosition { Left, Centre, Right }
    private NewWeaponPosition newWeaponPosition = NewWeaponPosition.Centre;

    private InputAction m_leftUIAction;
    private InputAction m_rightUIAction;
    private InputAction m_selectUIAction;

    private void Awake()
    {
        // Singleton pattern 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InstaHideCanvas();
    }

    public override void OnShowCanvas()
    {
        base.OnShowCanvas();

        SetInteractionActions();

        // get the current local player gotchi id
        var selectedGotchiId = GotchiDataManager.Instance.GetSelectedGotchiId();

        // set the new found weapon card
        m_newWeapon.Init(newWeaponNameEnum, selectedGotchiId);

        // now we need to get the local player gotchi and set the lh and rh weapon cards
        var playerEquipments = FindObjectsByType<PlayerEquipment>(FindObjectsSortMode.None);
        foreach (var playerEquipment in playerEquipments)
        {
            var networkObject = playerEquipment.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsLocalPlayer)
            {
                m_localPlayer = networkObject.GetComponent<PlayerController>();
                m_lhWeapon.Init(playerEquipment.LeftHand.Value, selectedGotchiId);
                m_rhWeapon.Init(playerEquipment.RightHand.Value, selectedGotchiId);
            }
        }

        // centre our new weapon
        ReCentre();
    }

    public override void OnUpdate()
    {
        if (m_leftUIAction != null && m_leftUIAction.WasPressedThisFrame())
        {
            SlideLeft();
        }

        if (m_rightUIAction != null && m_rightUIAction.WasPressedThisFrame())
        {
            SlideRight();
        }

        if (m_selectUIAction != null && m_selectUIAction.WasPressedThisFrame())
        {
            if (newWeaponPosition == NewWeaponPosition.Left)
            {
                activeWeaponSwap.SwapWeapon(Hand.Left, newWeaponNameEnum, m_localPlayer);
            }
            else if (newWeaponPosition == NewWeaponPosition.Right)
            {
                activeWeaponSwap.SwapWeapon(Hand.Right, newWeaponNameEnum, m_localPlayer);
            }
        }
    }

    protected void SetInteractionActions()
    {
        var playerInputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (var playerInput in playerInputs)
        {
            var networkObject = playerInput.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsLocalPlayer)
            {
                m_leftUIAction = playerInput.actions["InUI/Left"];
                m_rightUIAction = playerInput.actions["InUI/Right"];
                m_selectUIAction = playerInput.actions["InUI/Select"];
            }
        }
    }

    void ReCentre()
    {
        newWeaponPosition = NewWeaponPosition.Centre;
        var rectTransform = m_newWeapon.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(0, rectTransform.anchoredPosition.y);
        SetSwapInfoText(newWeaponPosition);
        SetOutlines(newWeaponPosition);
    }

    void SlideLeft()
    {
        if (newWeaponPosition == NewWeaponPosition.Right)
        {
            var rectTransform = m_newWeapon.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(0, 0.2f);
            newWeaponPosition = NewWeaponPosition.Centre;
        }

        else if (newWeaponPosition == NewWeaponPosition.Centre)
        {
            var rectTransform = m_newWeapon.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(-173, 0.2f);
            newWeaponPosition = NewWeaponPosition.Left;
        }

        SetSwapInfoText(newWeaponPosition);
        SetOutlines(newWeaponPosition);
    }

    void SlideRight()
    {
        if (newWeaponPosition == NewWeaponPosition.Left)
        {
            var rectTransform = m_newWeapon.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(0, 0.2f);
            newWeaponPosition = NewWeaponPosition.Centre;
        }

        else if (newWeaponPosition == NewWeaponPosition.Centre)
        {
            var rectTransform = m_newWeapon.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(173, 0.2f);
            newWeaponPosition = NewWeaponPosition.Right;
        }

        SetSwapInfoText(newWeaponPosition);
        SetOutlines(newWeaponPosition);
    }

    void SetSwapInfoText(NewWeaponPosition pos)
    {
        if (pos == NewWeaponPosition.Left)
        {
            m_swapInfoText.text = "Press F to swap out existing left hand weapon";
        }
        else if (pos == NewWeaponPosition.Right)
        {
            m_swapInfoText.text = "Press F to swap out existing right hand weapon";
        }
        else
        {
            m_swapInfoText.text = "Press left/right to swap new weapon for existing, or, F to leave it on the ground";
        }
    }

    void SetOutlines(NewWeaponPosition pos)
    {
        var newColor = m_newWeapon.GetComponent<Outline>().effectColor;
        var lhColor = m_lhWeapon.GetComponent<Outline>().effectColor;
        var rhColor = m_rhWeapon.GetComponent<Outline>().effectColor;

        if (pos == NewWeaponPosition.Left)
        {
            newColor.a = 1;
            lhColor.a = 0.1f;
            rhColor.a = 1;
        }
        else if (pos == NewWeaponPosition.Right)
        {
            newColor.a = 1;
            lhColor.a = 1;
            rhColor.a = 0.1f;
        }
        else
        {
            newColor.a = 0.1f;
            lhColor.a = 1;
            rhColor.a = 1;
        }

        m_newWeapon.GetComponent<Outline>().effectColor = newColor;
        m_lhWeapon.GetComponent<Outline>().effectColor = lhColor;
        m_rhWeapon.GetComponent<Outline>().effectColor = rhColor;
    }
}
