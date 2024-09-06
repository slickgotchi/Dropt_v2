using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dropt
{
    public class EnemyAI_Snail : EnemyAI
    {
        public float DirectionChangeInterval = 3f;
        public float DirectionChangeIntervalVariance = 1f;
        public float MaxRoamDistance = 10f;

        private float m_directionChangeTimer = 0f;
        private Vector3 m_direction;

        private NetworkCharacter m_networkCharacter;

        private void Awake()
        {
            m_networkCharacter = GetComponent<NetworkCharacter>();
        }

        public override void OnHandleRoam(float dt)
        {
            m_directionChangeTimer -= dt;
            if (m_directionChangeTimer < 0f)
            {
                m_directionChangeTimer = UnityEngine.Random.Range(DirectionChangeInterval - DirectionChangeIntervalVariance, DirectionChangeInterval + DirectionChangeIntervalVariance);
                m_direction = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0);
                m_direction = m_direction.normalized;
            }

            // roam
            transform.position += m_direction * m_networkCharacter.MoveSpeed.Value * RoamSpeedMultiplier * dt;
        }
    }
}
