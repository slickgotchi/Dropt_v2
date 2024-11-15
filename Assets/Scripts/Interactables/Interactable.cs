using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using UnityEngine.InputSystem;

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
    //public GameObject PopupCanvasPrefab;
    //public Vector3 PopupCanvasOffset;
    public bool isDisablePlayerInputWhileActive;

    //private GameObject m_popupCanvas;
    private Slider m_holdSlider;
    private Animator m_popupAnimator;
    private bool m_isPopupVisible = false;

    protected bool m_isOpen = false;

    public string InteractionText = "";

    [HideInInspector] public Status status;
    [HideInInspector] public ulong playerNetworkObjectId;

    // hold timer variables
    private float k_holdDuration = 0.5f;
    private float m_holdTimer = -0.1f;
    private float k_holdCooldownDuration = 2f;
    private float m_holdCooldownTimer = 0f;

    // local player
    private PlayerPrediction m_localPlayerPrediction;
    private PlayerInput m_localPlayerInput;
    private InputAction m_interactAction;
    private InputAction m_interactUIAction;

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
        base.OnNetworkSpawn();

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
        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null || !playerNetworkObject.IsLocalPlayer) return;

        status = Status.Active;

        if (IsClient)
        {
            playerNetworkObjectId = playerNetworkObject.NetworkObjectId;
            SetPlayerNetworkObjectIdServerRpc(playerNetworkObjectId);
        }

        OnTriggerStartInteraction();

        // display players press/hold canvas
        var interactHoldCanvas = player.transform.Find("InteractHoldCanvas");
        var interactPressCanvas = player.transform.Find("InteractPressCanvas");
        var isLocalPlayer = player.GetComponent<NetworkObject>().IsLocalPlayer;

        if (interactableType == InteractableType.Hold && interactHoldCanvas != null && isLocalPlayer)
        {
            m_popupAnimator = interactHoldCanvas.GetComponentInChildren<Animator>();
            m_holdSlider = interactHoldCanvas.GetComponentInChildren<Slider>();
            m_holdSlider.value = 0;
        }
        else if (interactableType == InteractableType.Press && interactPressCanvas != null && isLocalPlayer)
        {
            m_popupAnimator = interactPressCanvas.GetComponentInChildren<Animator>();
        }

        if (m_popupAnimator == null)
        {
            Debug.LogError("A popup animator has not been assigned to an interactable");
            return;
        }

        m_popupAnimator.Play("Show");
        PlayerHUDCanvas.Singleton.ShowInteractionPanel(InteractionText);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNetworkObjectIdServerRpc(ulong playerObjectId)
    {
        playerNetworkObjectId = playerObjectId;
    }

    private void Update()
    {
        TryGetLocalPlayerPrediction();

        m_pressTimer -= Time.deltaTime;

        if (status == Status.Inactive) return;

        OnTriggerUpdateInteraction();

        if (interactableType == InteractableType.Press)
        {
            if ((m_interactAction.IsPressed() || m_interactUIAction.IsPressed()) && m_pressTimer < 0
                && !PlayerInputMapSwitcher.Instance.IsSwitchTooRecent())
            {
                m_pressTimer = k_pressInterval;

                if (!m_isOpen)
                {
                    OnPressOpenInteraction();
                    if (m_popupAnimator != null) m_popupAnimator.Play("Hide");
                    PlayerHUDCanvas.Singleton.HideInteractionPanel();
                }
                else
                {
                    OnPressCloseInteraction();
                    if (m_popupAnimator != null) m_popupAnimator.Play("Show");
                    PlayerHUDCanvas.Singleton.ShowInteractionPanel(InteractionText);
                }

                m_isOpen = !m_isOpen;
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
                PlayerHUDCanvas.Singleton.HideInteractionPanel();
            }
        }

    }

    public void ExternalCanvasClosed()
    {
        OnPressCloseInteraction();
        if (m_popupAnimator != null) m_popupAnimator.Play("Show");
        PlayerHUDCanvas.Singleton.ShowInteractionPanel(InteractionText);
    }

    protected void TryGetLocalPlayerPrediction()
    {
        if (!IsClient) return;
        if (m_localPlayerPrediction != null) return;

        var playerPredictions = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var playerPrediction in playerPredictions)
        {
            if (playerPrediction.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                m_localPlayerPrediction = playerPrediction;
                m_localPlayerInput = playerPrediction.GetComponent<PlayerInput>();
                m_interactAction = m_localPlayerInput.actions["InGame/Generic_Interact"];
                m_interactUIAction = m_localPlayerInput.actions["InUI/Select"];
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null || !playerNetworkObject.IsLocalPlayer) return;

        status = Status.Inactive;

        playerNetworkObjectId = 0;

        m_isOpen = false;

        OnTriggerFinishInteraction();

        if (m_popupAnimator != null) m_popupAnimator.Play("Hide");
        PlayerHUDCanvas.Singleton.HideInteractionPanel();
    }

    public ulong GetLocalPlayerNetworkObjectId()
    {
        return m_localPlayerPrediction.GetComponent<NetworkObject>().NetworkObjectId;
    }

    public bool IsLocalPlayerNetworkObjectId(ulong playerNetworkObjectId)
    {
        return playerNetworkObjectId == GetLocalPlayerNetworkObjectId();
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
}
