using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombSnailController : NetworkBehaviour
{
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
                var attackAbility = GetComponent<EnemyAbilities>().PrimaryAttack;
                GetComponent<EnemyAbilities>().TryActivate(attackAbility, gameObject, null);
            }
        }
    }
}
