using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        NavMesh.avoidancePredictionTime = 3;
    }

    private void Update()
    {
        agent.SetDestination(new Vector3(0, 0, 0));
    }
}
