using Unity.Netcode;
using UnityEngine;
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
    public bool isDisablePlayerInputWhileActive;
    public string interactionText = "";

    [HideInInspector] public Status status;
    [HideInInspector] public ulong playerNetworkObjectId;
    protected ulong localPlayerNetworkObjectId;
    protected PlayerController localPlayerController;

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

    public virtual void OnTriggerEnter2DInteraction() { }
    public virtual void OnTriggerUpdateInteraction() { }
    public virtual void OnTriggerExit2DInteraction() { }

    public virtual void OnInteractHoldStart() { }
    public virtual void OnInteractHoldUpdate(float alpha) { }
    public virtual void OnInteractHoldFinish() { }

    public virtual void OnInteractPress() { }

    public virtual void OnUpdate() { }

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

    public PlayerController GetPlayerController(ulong playerNetworkObjectId)
    {
        var playerObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (playerObject == null) return null;

        return playerObject.GetComponent<PlayerController>();
    }

    // client only (and local player only) code
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!IsClient) return;

        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null || !playerNetworkObject.IsLocalPlayer) return;

        status = Status.Active;

        localPlayerController = player.GetComponent<PlayerController>();
        localPlayerNetworkObjectId = playerNetworkObject.NetworkObjectId;

        OnTriggerEnter2DInteraction();

        // reset player hud hold slider
        if (interactableType == InteractableType.Hold)
        {
            PlayerHUDCanvas.Instance.SetInteractHoldSliderValue(0);
        }
    }

    // Update only occurs for the LocalPlayer
    private void Update()
    {
        // this function just helps get the interact ui input actions
        TryGetLocalPlayerPrediction();
        if (m_localPlayerPrediction == null) return;

        // only run the below code if we are the local player and in client mode
        if (!m_localPlayerPrediction.GetComponent<NetworkObject>().IsLocalPlayer || !IsClient) return;

        // trigger updates for children
        OnUpdate();

        m_pressTimer -= Time.deltaTime;

        if (status == Status.Inactive) return;

        // if we are not the closest interactable, don't do any of the below code
        if (!IsClosestInteractable()) return;

        OnTriggerUpdateInteraction();

        if (interactableType == InteractableType.Press)
        {
            if ((m_interactAction.IsPressed() || m_interactUIAction.IsPressed()) && m_pressTimer < 0
                && !PlayerInputMapSwitcher.Instance.IsSwitchTooRecent())
            {
                m_pressTimer = k_pressInterval;
                OnInteractPress();
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
                OnInteractHoldStart();
            }

            // update if f is pressed
            //if (Input.GetKey(KeyCode.F)) m_holdTimer += Time.deltaTime;
            if (m_localPlayerPrediction.IsInteracting) m_holdTimer += Time.deltaTime;
            else m_holdTimer = 0f;
            float alpha = m_holdTimer / k_holdDuration;
            OnInteractHoldUpdate(alpha);

            // update sliderslider
            //if (m_holdSlider != null) m_holdSlider.value = m_holdTimer / k_holdDuration;
            PlayerHUDCanvas.Instance.SetInteractHoldSliderValue(m_holdTimer / k_holdDuration);

            // check if hold complete
            if (m_holdTimer > k_holdDuration)
            {
                m_holdCooldownTimer = k_holdCooldownDuration;
                OnInteractHoldFinish();
                m_holdTimer = -0.1f;
            }
        }
    }

    bool IsClosestInteractable()
    {
        if (localPlayerController == null) return false;

        var interactables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        var closestDist = 1e9;
        Interactable closestInteractable = null;
        foreach (var interactable in interactables)
        {
            var dist = math.distance(localPlayerController.transform.position, interactable.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestInteractable = interactable;
            }
        }

        return closestInteractable.gameObject == this.gameObject;
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
        if (!IsClient) return;

        var cameraFollower = collider.GetComponent<CameraFollowerAndPlayerInteractor>();
        if (cameraFollower == null) return;

        var player = cameraFollower.Player;
        if (player == null) return;

        var playerNetworkObject = player.GetComponent<NetworkObject>();
        if (playerNetworkObject == null || !playerNetworkObject.IsLocalPlayer) return;

        status = Status.Inactive;

        localPlayerNetworkObjectId = 0;
        //playerNetworkObjectId = 0;

        OnTriggerExit2DInteraction();
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