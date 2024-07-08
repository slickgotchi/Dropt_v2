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
    public VariableReference<float> PursueSpeed = 3.0f;
    public VariableReference<float> StopShortDistance = 1.5f;

    private float attackCooldown = 0;
    private bool isAttackTime = false;

    protected override void OnStart()
    {
        attackCooldown = 2f;
    }

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // ensure we still have a valid context and target
        if (Context == null || Context.Target == null) return UpdateStatus.Running;

        // Note: this should only run server side (because we remove NavMeshAgent from our client side players)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;

        // try get target
        var target = Context.Target.GetComponent<Transform>();
        if (target == null) return UpdateStatus.Running;

        var self = navMeshAgent.transform;

        Vector3 dir = (target.position - self.position).normalized;

        navMeshAgent.SetDestination(target.position - dir * StopShortDistance);
        navMeshAgent.speed = 3f;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0)
        {
            isAttackTime = true;
            attackCooldown = 2f;
        }

        if (isAttackTime)
        {
            Debug.Log("Attack");

            var attack = GameObject.Instantiate(GameObject.GetComponent<EnemyAttack>().AttackPrefab);
            attack.transform.position = self.position + new Vector3(0, 0.35f, 0f);
            attack.transform.rotation = PlayerAbility.GetRotationFromDirection(dir);
            attack.GetComponent<NetworkObject>().Spawn();
            attack.GetComponent<NetworkObject>().TrySetParent(GameObject);

            isAttackTime = false;
        }

        return UpdateStatus.Running;
    }
}
