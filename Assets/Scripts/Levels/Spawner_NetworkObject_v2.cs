using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner_NetworkObject_v2 : MonoBehaviour
{
    [System.Serializable]
    public class SpawnOption
    {
        public GameObject NetworkObjectPrefab;
        public float Chance = 1;
    }

    [SerializeField]
    public float nillSpawnChance = 0f;

    // spawn options
    [SerializeField]
    public List<SpawnOption> spawnOptions = new List<SpawnOption>();

    // spawner id is used to link spawned groups together for interactive behaviours (e.g. spawn once all of a certain ID is destroyed
    [SerializeField]
    public int spawnerId = -1;

    [HideInInspector] public GameObject spawnedNetworkObject;

    /// <summary>
    /// Use this to normalize the spawn chances of the entire list (if sum of all spawn chances > 1)
    /// </summary>
    private void Normalize()
    {
        // get sum of all chances
        float sum = nillSpawnChance;
        for (int i = 0; i < spawnOptions.Count; i++)
        {
            sum += spawnOptions[i].Chance;
        }

        // if sum < 1, don't do any normalization
        if (sum <= 1) return;

        // don't allow division by zero
        if (sum <= 0) sum = 1;

        // divide all chances by the sum
        nillSpawnChance /= sum;
        for (int i = 0; i < spawnOptions.Count; i++)
        {
            spawnOptions[i].Chance /= sum;
        }
    }

    /// <summary>
    /// Returns a random prefab from the spawnOptions list (based on the chances)
    /// </summary>
    /// <returns></returns>
    public GameObject GetRandom()
    {
        if (spawnOptions.Count <= 0) return null;

        // normalize spawnOptions
        Normalize();

        // get random var
        float rand = UnityEngine.Random.Range(0f, 1f);

        // iterate over spawn options (starting with the nillchance)
        if (rand < nillSpawnChance) return null;
        else rand -= nillSpawnChance;

        // now iterate over spawn options
        for (int i = 0; i < spawnOptions.Count; i++)
        {
            if (rand < spawnOptions[i].Chance)
            {
                return spawnOptions[i].NetworkObjectPrefab;
            }
            else
            {
                rand -= spawnOptions[i].Chance;
            }
        }

        return null;
    }
}
