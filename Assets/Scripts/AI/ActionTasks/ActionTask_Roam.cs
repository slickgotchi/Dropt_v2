using CarlosLab.Common;
using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class ActionTask_Roam : ActionTask
{
    public VariableReference<float> MaxDirectionChangeTime = 5.0f;
    public VariableReference<float> MinDirectionChangeTime = 2.0f;
    public VariableReference<float> MaxRoamSpeed = 2f;
    public VariableReference<float> MinRoamSpeed = 1f;
    public VariableReference<float> TargetDistance = 20f;

    private float m_changeDirectionTimer = 0;

    protected override void OnAwake()
    {
        m_changeDirectionTimer = 0;
    }

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // this should only run server side (because we only add NavMeshAgent to server spawns)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;

        m_changeDirectionTimer -= deltaTime;

        if (m_changeDirectionTimer <= 0)
        {
            m_changeDirectionTimer += UnityEngine.Random.Range(MinDirectionChangeTime, MaxDirectionChangeTime);

            var currentPosition = navMeshAgent.transform.position;

            var dir = new float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
            dir = math.normalizesafe(dir);  

            var targetDestination = currentPosition + new Vector3(dir.x,dir.y,0) * TargetDistance;
            navMeshAgent.SetDestination(targetDestination);
            navMeshAgent.speed = UnityEngine.Random.Range(MinRoamSpeed, MaxRoamSpeed);
        }

        return UpdateStatus.Running;
    }
}
