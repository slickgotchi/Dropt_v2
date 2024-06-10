using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SnailAgentFacade : UtilityAgentFacade
{
    private NavMeshAgent m_navMeshAgent;

    private void Awake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
    }
}
