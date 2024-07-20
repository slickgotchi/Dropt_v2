using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;
using CarlosLab.Common;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

public class ActionTask_Attack : ActionTask
{
    private bool m_isActivated = false;

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // ensure we still have a valid context and target
        if (Context == null || Context.Target == null) return UpdateStatus.Running;

        // Note: this should only run server side (because we remove NavMeshAgent from our client side players)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;
        navMeshAgent.stoppingDistance = 0;
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        // try get target
        var target = Context.Target.GetComponent<Transform>();
        if (target == null) return UpdateStatus.Running;

        if (!m_isActivated)
        {
            var attackAbility = GameObject.GetComponent<EnemyAbilities>().PrimaryAttack;
            m_isActivated = GameObject.GetComponent<EnemyAbilities>().TryActivate(attackAbility, GameObject, target.gameObject);
        } else
        {
            if (!GameObject.GetComponent<EnemyAbilities>().IsAbilityRunning())
            {
                m_isActivated = false;
                return UpdateStatus.Success;
            }
        }

        return UpdateStatus.Running;
    }
}
