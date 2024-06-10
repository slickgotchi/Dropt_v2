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
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_navMeshAgent.updateRotation = false;
        m_navMeshAgent.updateUpAxis = false;

        m_utilityAgentController = GetComponent<UtilityAgentController>();
        m_utilityAgentFacade = GetComponent<UtilityAgentFacade>();

        // register with the utility world
        m_utilityAgentController.Register(UtilityWorldSingleton.Instance.World);
    }

    public override void OnNetworkDespawn()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
