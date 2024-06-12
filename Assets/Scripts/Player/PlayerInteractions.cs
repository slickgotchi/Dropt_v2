using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractions : NetworkBehaviour
{
    private NetworkVariable<int> m_interactingNetworkObjectId;

    private int m_interactablesLayer;

    private void Awake()
    {
        m_interactablesLayer = 1 << LayerMask.NameToLayer("Interactable");
        m_interactingNetworkObjectId = new NetworkVariable<int>(-1);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            var interactionHit = Physics2D.OverlapCircle(
                transform.position + new Vector3(0, 0.6f, 0),
                0.5f,
                m_interactablesLayer);

            if (interactionHit != null) {
                var networkObject = interactionHit.gameObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    m_interactingNetworkObjectId.Value = (int)networkObject.NetworkObjectId;
                }
            } else
            {
                m_interactingNetworkObjectId.Value = -1;
            }
        }

        if (IsLocalPlayer)
        {
            InteractableUICanvas.Instance.InteractTextbox.SetActive(m_interactingNetworkObjectId.Value != -1);

            // INSERT LOGIC FOR HANDLING DIFFERENT INTERACTABLES
        }
    }
}
