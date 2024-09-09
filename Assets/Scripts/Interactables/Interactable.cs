using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public enum Status
    {
        Inactive,
        Active,
    }

    public enum InteractType
    {
        Press, Hold
    }

    public InteractType type = InteractType.Press;
    public GameObject PopupCanvasPrefab;
    public Vector3 PopupCanvasOffset;

    private GameObject m_popupCanvas;
    
    [HideInInspector] public Status status;
    [HideInInspector] public ulong playerNetworkObjectId;

    public virtual void OnStartInteraction() { }

    public virtual void OnUpdateInteraction() { }

    public virtual void OnFinishInteraction() { }

    private void Awake()
    {
        if (PopupCanvasPrefab != null)
        {
            m_popupCanvas = Instantiate(PopupCanvasPrefab);
            var animator = m_popupCanvas.GetComponentInChildren<Animator>();
            if (animator != null) animator.Play("Hidden");
            m_popupCanvas.transform.position = transform.position + PopupCanvasOffset;
        }
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
        if (!collider.HasComponent<CameraFollowerAndPlayerInteractor>()) return;

        status = Status.Active;

        playerNetworkObjectId = collider.GetComponent<CameraFollowerAndPlayerInteractor>()
            .Player.GetComponent<NetworkObject>().NetworkObjectId;

        OnStartInteraction();

        if (m_popupCanvas != null)
        {
            var animator = m_popupCanvas.GetComponentInChildren<Animator>();
            if (animator != null) animator.Play("Show");
        }
    }

    private void Update()
    {
        if (status == Status.Inactive) return;

        OnUpdateInteraction();
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.HasComponent<CameraFollowerAndPlayerInteractor>()) return;

        status = Status.Inactive;

        playerNetworkObjectId = 0;

        OnFinishInteraction();

        if (m_popupCanvas != null)
        {
            var animator = m_popupCanvas.GetComponentInChildren<Animator>();
            if (animator != null) animator.Play("Hide");
        }
    }
}
