using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpriteFlipToNavMeshAgentVelocity : MonoBehaviour
{
    public SpriteRenderer SpriteToFlip;

    private NavMeshAgent m_navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        if (m_navMeshAgent == null)
        {
            Debug.Log("No NavMeshAgent assigned to SpriteFlipToNavMeshAgentVelocity GameObject");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        SpriteToFlip.flipX = m_navMeshAgent.velocity.x > 0 ? false : true;
    }
}
