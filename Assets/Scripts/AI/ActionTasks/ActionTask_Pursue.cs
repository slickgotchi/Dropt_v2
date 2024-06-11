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

    protected override void OnAwake()
    {
    }

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // this should only run server side (because we only add NavMeshAgent to server spawns)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;

        var target = Context.Target.GetComponent<Transform>();
        if (!target) return UpdateStatus.Running;

        var self = navMeshAgent.transform;

        Vector3 dir = (self.position - target.position).normalized;

        navMeshAgent.SetDestination(target.position + dir * StopShortDistance);
        navMeshAgent.speed = PursueSpeed;
        

        return UpdateStatus.Running;
    }
}
