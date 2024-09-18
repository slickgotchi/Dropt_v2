using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;

public class Interactable : NetworkBehaviour
{
    public enum Status
    {
        Inactive,
        Active,
    }

    public enum InteractableType
    {
        Press, Hold
    }

    public InteractableType interactableType = InteractableType.Press;
    public GameObject PopupCanvasPrefab;
    public Vector3 PopupCanvasOffset;
    public bool isDisablePlayerInputWhileActive;

    private GameObject m_popupCanvas;
    private Slider m_holdSlider;
    private Animator m_popupAnimator;
    
    [HideInInspector] public Status status;
    [HideInInspector] public ulong playerNetworkObjectId;

    // hold timer variables
    private float k_holdDuration = 0.5f;
    private float m_holdTimer = -0.1f;
    private float k_holdCooldownDuration = 2f;
    private float m_holdCooldownTimer = 0f;

    // local player
    private PlayerPrediction m_localPlayerPrediction;

    // float to validate distance to interactable
    private float k_validateInteractionDistance = 3f;

    public virtual void OnTriggerStartInteraction() { }
    public virtual void OnTriggerUpdateInteraction() { }
    public virtual void OnTriggerFinishInteraction() { }

    public virtual void OnHoldStartInteraction() { }
    public virtual void OnHoldUpdateInteraction(float alpha) { }
    public virtual void OnHoldFinishInteraction() { }

    public virtual void OnPressOpenInteraction() { }
    public virtual void OnPressCloseInteraction() { }

    private float k_pressInterval = 0.5f;
    private float m_pressTimer = 0.5f;

    public override void OnNetworkSpawn()
    {
        if (PopupCanvasPrefab != null && IsClient)
        {
            m_popupCanvas = Instantiate(PopupCanvasPrefab);
            m_popupCanvas.transform.position = transform.position + PopupCanvasOffset;

            m_holdSlider = m_popupCanvas.GetComponentInChildren<Slider>();
            if (m_holdSlider != null) m_holdSlider.value = 0;

            m_popupAnimator = m_popupCanvas.GetComponentInChildren<Animator>();
        }

        m_holdTimer = -0.1f;
        m_holdCooldownTimer = 0f;
    }

    public bool IsPlayerIdLocal(ulong playerNetworkObjectId)
    {
        return NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].IsLocalPlayer;
    }

    public PlayerController GetPlayerController()
    {
        var playerObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (playerObject == null) return null;

        return playerObject.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsClient) return;
        if (!collider.HasComponent<CameraFollowerAndPlayerInteractor>()) return;

        status = Status.Active;

        playerNetworkObjectId = collider.GetComponent<CameraFollowerAndPlayerInteractor>()
            .Player.GetComponent<NetworkObject>().NetworkObjectId;

        OnTriggerStartInteraction();

        if (m_popupCanvas != null)
        {
            if (m_popupAnimator != null) m_popupAnimator.Play("Show");
        }
    }

    private bool m_isOpen = false;

    private void Update()
    {
        TryGetLocalPlayerPrediction();
        
        m_pressTimer -= Time.deltaTime;

        if (status == Status.Inactive) return;

        OnTriggerUpdateInteraction();

        if (interactableType == InteractableType.Press)
        {
            //if (Input.GetKeyDown(KeyCode.F))
            if (m_localPlayerPrediction.IsInteracting && m_pressTimer < 0)
            {
                m_pressTimer = k_pressInterval;

                if (!m_isOpen)
                {
                    OnPressOpenInteraction();
                    if (m_popupAnimator != null) m_popupAnimator.Play("Hide");
                    m_isOpen = true;
                }
                else
                {
                    OnPressCloseInteraction();
                    if (m_popupAnimator != null) m_popupAnimator.Play("Show");
                    m_isOpen = false;
                }
            }
        }
        else if (interactableType == InteractableType.Hold)
        {
            // check for hold cooldown timer
            m_holdCooldownTimer -= Time.deltaTime;
            if (m_holdCooldownTimer > 0) return;

            // check for first time
            if (m_holdCooldownTimer <= 0 && m_holdTimer <= 0)
            {
                OnHoldStartInteraction();
            }

            // update if f is pressed
            //if (Input.GetKey(KeyCode.F)) m_holdTimer += Time.deltaTime;
            if (m_localPlayerPrediction.IsInteracting) m_holdTimer += Time.deltaTime;
            else m_holdTimer = 0f;
            var alpha = m_holdTimer / k_holdDuration;
            OnHoldUpdateInteraction(alpha);

            // update sliderslider
            if (m_holdSlider != null) m_holdSlider.value = m_holdTimer / k_holdDuration;

            // check if hold complete
            if (m_holdTimer > k_holdDuration)
            {
                m_holdCooldownTimer = k_holdCooldownDuration;
                OnHoldFinishInteraction();

                m_holdTimer = -0.1f;

                if (m_popupAnimator != null) m_popupAnimator.Play("Hide");
            }
        }

    }

    void TryGetLocalPlayerPrediction()
    {
        if (!IsClient) return;
        if (m_localPlayerPrediction != null) return;

        var playerPredictions = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var playerPrediction in playerPredictions)
        {
            if (playerPrediction.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                m_localPlayerPrediction = playerPrediction;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.HasComponent<CameraFollowerAndPlayerInteractor>()) return;

        status = Status.Inactive;

        playerNetworkObjectId = 0;

        OnTriggerFinishInteraction();

        if (m_popupCanvas != null)
        {
            if (m_popupAnimator != null) m_popupAnimator.Play("Hide");
        }
    }

    public bool IsValidInteraction(ulong networkObjectId)
    {
        // try get player controller
        var playerController = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<PlayerController>();
        if (playerController == null) return false;

        // is player controller within "check" radius of interactor
        var distance = math.distance(playerController.transform.position, transform.position);
        return distance < k_validateInteractionDistance; 
    }

    protected void SetPlayerInputEnabled(bool isEnabled)
    {
        var player = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (player == null)
        {
            Debug.LogWarning("No valid player object for SetPlayerInputEnabled()");
            return;
        }

        var playerPrediction = player.GetComponent<PlayerPrediction>();
        playerPrediction.IsInputEnabled = isEnabled;
    }
}
