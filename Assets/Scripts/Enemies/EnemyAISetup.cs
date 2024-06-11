using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAISetup : NetworkBehaviour
{
    private NavMeshAgent m_navMeshAgent;
    private UtilityAgentController m_utilityAgentController;
    private UtilityAgentFacade m_utilityAgentFacade;

    public override void OnNetworkSpawn()
    {
        // Utility AI
        m_utilityAgentController = GetComponent<UtilityAgentController>();
        m_utilityAgentFacade = GetComponent<UtilityAgentFacade>();

        // only add nav mesh agent on the server
        if (IsServer || IsHost)
        {
            // NavMeshAgent
            m_navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.updateUpAxis = false;
            m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_navMeshAgent.enabled = true;

            // register with the utility world
            m_utilityAgentController.Register(UtilityWorldSingleton.Instance.World);
        } else 
        {
            m_utilityAgentController.enabled = false;
            m_utilityAgentFacade.enabled = false;
            return;
        }
    }
}
