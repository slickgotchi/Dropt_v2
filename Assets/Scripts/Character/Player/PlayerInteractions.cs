using Unity.Netcode;
using UnityEngine;

public class PlayerInteractions : NetworkBehaviour
{
    private int m_interactablesLayer;

    private Interactable m_interactable;

    private ulong m_playerNetworkObjectId = 0;

    private void Awake()
    {
        m_interactablesLayer = 1 << LayerMask.NameToLayer("Interactable");
    }

    public override void OnNetworkSpawn()
    {
        m_playerNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
    }

    void Update()
    {
        if (!IsLocalPlayer) return;
        if (!IsSpawned) return;

        // test for any interaction hits
        var interactionHit = Physics2D.OverlapCircle(
            transform.position + new Vector3(0, 0.6f, 0),
            0.5f,
            m_interactablesLayer);
        if (interactionHit == null || !interactionHit.HasComponent<Interactable>())
        {
            // finish interactable if its active
            if (m_interactable != null && m_interactable.status == Interactable.Status.Active)
            {
                m_interactable.status = Interactable.Status.Inactive;
                m_interactable.OnFinishInteraction();
                m_interactable.playerNetworkObjectId = 0;
                m_interactable = null;
            }

            return;
        }

        // got a interact so lets process it
        m_interactable = interactionHit.GetComponent<Interactable>();

        if (m_interactable.status == Interactable.Status.Inactive)
        {
            m_interactable.status = Interactable.Status.Active;
            m_interactable.playerNetworkObjectId = GetComponent<NetworkObject>().NetworkObjectId;
            m_interactable.OnStartInteraction();
        }

        m_interactable.OnUpdateInteraction();
    }
}
