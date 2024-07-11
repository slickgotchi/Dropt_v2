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
        // ensure we still have a valid context and target
        if (Context == null || Context.Target == null) return UpdateStatus.Running;

        // Note: this should only run server side (because we remove NavMeshAgent from our client side players)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;
        navMeshAgent.isStopped = false;

        // try get target
        var target = Context.Target.GetComponent<Transform>();
        if (target == null) return UpdateStatus.Running;

        var self = navMeshAgent.transform;

        Vector3 dir = (self.position - target.position).normalized;

        navMeshAgent.SetDestination(target.position + dir * StopShortDistance);
        navMeshAgent.speed = PursueSpeed;
        

        return UpdateStatus.Running;
    }
}
