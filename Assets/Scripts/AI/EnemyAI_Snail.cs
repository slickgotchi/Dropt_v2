using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace Dropt
{
    public class EnemyAI_Snail : EnemyAI
    {
        


        private NetworkCharacter m_networkCharacter;
        private NavMeshAgent m_navMeshAgent;

        private void Awake()
        {
            m_networkCharacter = GetComponent<NetworkCharacter>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private bool m_isSpawned = false;

        public override void OnSpawnUpdate(float dt)
        {
            if (!m_isSpawned)
            {
                GetComponent<Animator>().Play("Snail_Unburrow");
                m_isSpawned = true;
            }
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoam(dt);   
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursue(dt);
        }
    }
}
