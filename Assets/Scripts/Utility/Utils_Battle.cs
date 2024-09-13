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

        public static Vector3 GetRandomSurroundPosition(Vector3 targetPosition, float minRadius, float maxRadius)
        {
            // Ensure the minRadius is not larger than maxRadius
            if (minRadius > maxRadius)
            {
                Debug.LogWarning("minRadius cannot be larger than maxRadius. Swapping values.");
                float temp = minRadius;
                minRadius = maxRadius;
                maxRadius = temp;
            }

            // Generate a random angle between 0 and 360 degrees (in radians)
            float randomAngle = Random.Range(0f, Mathf.PI * 2);

            // Generate a random distance between minRadius and maxRadius
            float randomDistance = Random.Range(minRadius, maxRadius);

            // Calculate the offset in X and Y direction using the angle and distance
            float offsetX = Mathf.Cos(randomAngle) * randomDistance;
            float offsetY = Mathf.Sin(randomAngle) * randomDistance;

            // Return the new position offset from the targetPosition
            return new Vector3(targetPosition.x + offsetX, targetPosition.y + offsetY, targetPosition.z);
        }

        public static bool CheckCircleCollision(Vector2 position, float radius, LayerMask layerMask = default)
        {
            // If no LayerMask is provided (default value), use all layers (~0)
            if (layerMask == default)
            {
                layerMask = ~0;  // ~0 includes all layers
            }

            // Perform an overlap circle check against the provided LayerMask
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius, layerMask);

            // Return true if any colliders are found
            return colliders.Length > 0;
        }
    }
}
