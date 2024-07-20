using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

public class ActionTask_SpawnSpiders : ActionTask
{
    bool m_isActivated = false;

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        if (!m_isActivated)
        {
            var spawnAbility = GameObject.GetComponent<EnemyAbilities>().PrimaryAttack;
            m_isActivated = GameObject.GetComponent<EnemyAbilities>()
                .TryActivate(spawnAbility, GameObject, null);

        }


        return UpdateStatus.Running;
    }
}
