using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerUtilityAI : NetworkBehaviour
{
    private UtilityEntityController m_entityController;
    private PlayerEntityFacade m_entityFacade;

    public override void OnNetworkSpawn()
    {
        m_entityController = GetComponent<UtilityEntityController>();
        m_entityFacade = GetComponent<PlayerEntityFacade>();

        // register utility entity if this is the server instance
        if (IsServer)
        {
            m_entityController.Register(UtilityWorldSingleton.Instance.World);

        }
        else if (IsClient && !IsHost)
        {
            m_entityController.enabled = false;
            m_entityFacade.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsServer && !IsHost && IsOwnedByServer)
        {
            m_entityFacade.Destroy();
        }
    }
}
