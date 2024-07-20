using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;

namespace Dropt.UtilityIntelligence
{
    public class MyIsSpawningInput : Input<bool>
    {
        protected override bool OnGetRawInput(InputContext context)
        {
            var enemyTransform = AgentFacade.GetComponent<Transform>();
            if (enemyTransform == null)
            {
                return false;
            }

            var enemyController = enemyTransform.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                return false;
            }

            return enemyController.IsSpawning;
        }
    }
}
