using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSnailController : NetworkBehaviour
{
    public GameObject explodeAbilityPrefab;

    private float m_explosionTimer = 3f;

    private void Update()
    {
        if (!IsServer) return;

        if (GetComponent<EnemyController>().IsArmed)
        {
            m_explosionTimer -= Time.deltaTime;

            // reduce detonation time
            if (m_explosionTimer <= 0)
            {
                // get enemy ai
                Dropt.EnemyAI enemyAI = GetComponent<Dropt.EnemyAI>();
                if (enemyAI == null) return;

                // instantiate an attack
                GameObject ability = Instantiate(explodeAbilityPrefab, transform.position, Quaternion.identity);

                // get enemy ability of attack
                EnemyAbility enemyAbility = ability.GetComponent<EnemyAbility>();
                if (enemyAbility == null) return;

                // initialise the ability
                ability.GetComponent<NetworkObject>().Spawn();
                enemyAbility.Activate(gameObject, enemyAI.NearestPlayer, enemyAI.AttackDirection, enemyAI.AttackDuration, enemyAI.PositionToAttack);
            }
        }
    }
}
