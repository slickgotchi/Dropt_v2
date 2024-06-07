using UnityEngine;

public static class DashCalcs
{
    public static Vector3 Dash(CapsuleCollider2D playerCollider, Vector3 startPosition, Vector3 direction, float distance)
    {
        int environmentWallLayerIndex = LayerMask.NameToLayer("EnvironmentWall");
        var environmentWallLayer = 1 << environmentWallLayerIndex;

        int environmentWaterLayerIndex = LayerMask.NameToLayer("EnvironmentWater");
        var environmentWaterLayer = 1 << environmentWaterLayerIndex;

        var combinedLayer = (1 << environmentWallLayerIndex) | (1 << environmentWaterLayerIndex);

        Vector2 capsuleOffset = playerCollider.offset;
        Vector3 adjustedStartPosition = startPosition + (Vector3)capsuleOffset;
        Vector3 adjustedDirection = direction.normalized;
        Vector3 initialFinishPosition = adjustedStartPosition + adjustedDirection * distance;

        // Perform initial collider cast to check for collisions along the start/finish path
        RaycastHit2D hit = Physics2D.CapsuleCast(adjustedStartPosition, playerCollider.size, playerCollider.direction, 0, adjustedDirection, distance, environmentWallLayer);
        Vector3 finishPosition = hit.collider != null ? hit.point : initialFinishPosition;

        float checkDistance = Vector3.Distance(adjustedStartPosition, finishPosition);

        int numDirections = 8;
        float increment = 0.1f;
        float maxDistance = 1f;

        while (checkDistance > 0)
        {
            // Test for collision at the finish position
            Collider2D[] hits = Physics2D.OverlapCapsuleAll(finishPosition, playerCollider.size, playerCollider.direction, 0, environmentWaterLayer);

            if (hits.Length == 0)
            {
                // No collision at the finish position, move player to finish position
                return finishPosition - (Vector3)capsuleOffset;
            }
            else
            {
                // Resolve collisions by moving out in different directions
                Vector3[] tryDirections = GenerateDirections(numDirections);

                foreach (var tryDirection in tryDirections)
                {
                    for (float dist = increment; dist <= maxDistance; dist += increment)
                    {
                        Vector3 tempPosition = finishPosition + tryDirection * dist;

                        // Check for collisions at the temporary position
                        Collider2D[] tempHits = Physics2D.OverlapCapsuleAll(tempPosition, playerCollider.size, playerCollider.direction, 0, environmentWaterLayer);
                        if (tempHits.Length == 0)
                        {
                            // No collision at the new position, this is our finish position
                            return tempPosition - (Vector3)capsuleOffset;
                        }
                    }
                }

                // If all directions are tried and there is still a collision, move back 0.2 units towards the start position
                checkDistance -= 0.2f;
                finishPosition = adjustedStartPosition + adjustedDirection * checkDistance;
            }
        }

        // If we exit the loop, it means the finish position is not valid
        return finishPosition - (Vector3)capsuleOffset;
    }

    private static Vector3[] GenerateDirections(int numDirections)
    {
        Vector3[] directions = new Vector3[numDirections];
        float angleStep = 360f / numDirections;

        for (int i = 0; i < numDirections; i++)
        {
            float angle = Mathf.Deg2Rad * (angleStep * i);
            directions[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0).normalized;
        }

        return directions;
    }

    private static float CalculateMaxDistance(Vector3 direction)
    {
        // Define max distance for each main direction
        float maxUp = 0.5f;
        float maxDown = 0.5f;
        float maxRight = 2f;
        float maxLeft = 2f;

        // Determine the weight of each component
        float upWeight = Mathf.Clamp01(direction.y);
        float rightWeight = Mathf.Clamp01(direction.x);
        float downWeight = Mathf.Clamp01(-direction.y);
        float leftWeight = Mathf.Clamp01(-direction.x);

        // Calculate the max distance based on the direction
        float maxDistance = Mathf.Lerp(
            Mathf.Lerp(maxUp, maxRight, rightWeight),
            Mathf.Lerp(maxDown, maxLeft, leftWeight),
            downWeight
        );

        return maxDistance;
    }
}