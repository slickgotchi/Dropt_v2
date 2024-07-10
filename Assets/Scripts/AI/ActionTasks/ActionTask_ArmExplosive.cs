using CarlosLab.Common;
using CarlosLab.UtilityIntelligence;
using UnityEngine;
using UnityEngine.AI;

public class ActionTask_ArmExplosive : ActionTask
{
    public VariableReference<float> PursueSpeed = 3.0f;
    public VariableReference<float> StopShortDistance = 1.5f;
    public VariableReference<float> DetonationTime = 3f;
    private bool m_isActivated = false;

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

        // move to target
        var self = navMeshAgent.transform;
        Vector3 dir = (self.position - target.position).normalized;
        navMeshAgent.SetDestination(target.position + dir * StopShortDistance);
        navMeshAgent.speed = PursueSpeed;

        // reduce detonation time
        DetonationTime.Value -= Time.deltaTime;
        Debug.Log("Detonation time: " + DetonationTime.Value);
        //
        if (DetonationTime.Value <= 0)
        {
            Debug.Log("Try activate explosion");
            var attackAbility = GameObject.GetComponent<EnemyAbilities>().PrimaryAttack;
            m_isActivated = GameObject.GetComponent<EnemyAbilities>().TryActivate(attackAbility, GameObject, target.gameObject);

            return UpdateStatus.Success;
        }

        return UpdateStatus.Running;
    }
}
