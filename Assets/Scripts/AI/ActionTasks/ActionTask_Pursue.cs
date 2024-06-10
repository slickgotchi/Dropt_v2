using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;
using CarlosLab.Common;
using UnityEngine.AI;
using Unity.Mathematics;

public class ActionTask_Pursue : ActionTask
{
    public VariableReference<float> PursueSpeed = 3.0f;
    public VariableReference<float> StopShortDistance = 1.5f;

    private NavMeshAgent m_navMeshAgent;

    protected override void OnAwake()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
    }

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        var target = Context.Target.GetComponent<Transform>();
        var self = m_navMeshAgent.transform;

        Vector3 dir = (self.position - target.position).normalized;

        m_navMeshAgent.SetDestination(target.position + dir * StopShortDistance);
        m_navMeshAgent.speed = PursueSpeed;
        

        return UpdateStatus.Running;
    }
}
