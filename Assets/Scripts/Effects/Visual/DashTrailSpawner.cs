using Unity.Mathematics;
using UnityEngine;

public class DashTrailSpawner : MonoBehaviour
{
    public GameObject dashTrailPrefab; // Reference to the DashTrail prefab

    public void DrawShadow(Vector3 startPosition, Vector3 finishPosition, int numberSpawns)
    {
        if (numberSpawns <= 0 || dashTrailPrefab == null)
        {
            Debug.LogWarning("Invalid number of spawns or dashTrailPrefab is not assigned.");
            return;
        }

        float totalDistance = Vector3.Distance(startPosition, finishPosition);
        float interval = totalDistance / (numberSpawns - 1);

        for (int i = 0; i < numberSpawns; i++)
        {
            Vector3 spawnPosition = Vector3.Lerp(startPosition, finishPosition, i / (float)(numberSpawns - 1));
            GameObject dashTrail = Instantiate(dashTrailPrefab, spawnPosition, Quaternion.identity);

            float lifeTime = Mathf.Lerp(0.2f, 0.5f, i / (float)(numberSpawns - 1)); // Interpolating lifetime between 1 and 5 seconds (you can adjust these values as needed)

            DashTrail dashTrailScript = dashTrail.GetComponent<DashTrail>();
            if (dashTrailScript != null)
            {
                dashTrailScript.destroyTimer = lifeTime;
                dashTrailScript.SetSpriteFromDirection(math.normalize(finishPosition - startPosition));
            }
            else
            {
                Debug.LogWarning("DashTrail script not found on dashTrailPrefab.");
            }
        }
    }
}
