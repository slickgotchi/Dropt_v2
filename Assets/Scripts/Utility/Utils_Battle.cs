using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dropt.Utils
{
    public static class Battle
    {
        public static Vector3 GetAttackCentrePosition(GameObject go)
        {
            var attackCentre = go.GetComponentInChildren<AttackCentre>();
            if (attackCentre == null)
            {
                return go.transform.position;
            }

            return attackCentre.transform.position;
        }

        public static Vector3 GetVectorFromAtoBAttackCentres(GameObject a, GameObject b)
        {
            var aCentre = a.GetComponentInChildren<AttackCentre>();
            var aCentrePos = aCentre == null ? a.transform.position : aCentre.transform.position;

            var bCentre = b.GetComponentInChildren<AttackCentre>();
            var bCentrePos = bCentre == null ? b.transform.position : bCentre.transform.position;

            return bCentrePos - aCentrePos;
        }
    }
}
