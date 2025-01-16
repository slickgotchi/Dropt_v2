using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

namespace Level
{
    // Spawner_SpawnOnDestroyGroup
    public partial class NetworkLevel : NetworkBehaviour
    {
        public void CreateSpawners_SpawnOnDestroyGroup()
        {
            var spawnOnDestroyGroupSpawners = new List<Spawner_SpawnOnDestroyGroup>(GetComponentsInChildren<Spawner_SpawnOnDestroyGroup>());

            for (int i = 0; i < spawnOnDestroyGroupSpawners.Count; i++)
            {
                // grab key spawn info
                var minNumberSpawns = spawnOnDestroyGroupSpawners[i].MinNumberSpawns;
                var maxNumberSpawns = spawnOnDestroyGroupSpawners[i].MaxNumberSpawns;
                var spawnPrefab = spawnOnDestroyGroupSpawners[i].SpawnOnDestroyPrefab;
                var spawnOffset = spawnOnDestroyGroupSpawners[i].Offset;
                var spawnChance = spawnOnDestroyGroupSpawners[i].SpawnChance;

                // iterate over spawner children
                var spawners = spawnOnDestroyGroupSpawners[i].GetComponentsInChildren<Spawner_NetworkObject_v2>();

                // Filter out spawners where `spawnedNetworkObject` is null
                var validSpawners = spawners.Where(spawner => spawner.spawnedNetworkObject != null).ToArray();
                var numValidSpawners = validSpawners.Length;

                // Pick a random number of spawns between min and max (inclusive)
                var randNumberSpawns = UnityEngine.Random.Range(minNumberSpawns, maxNumberSpawns + 1);

                // Ensure we don't select more spawners than are available
                randNumberSpawns = Mathf.Min(randNumberSpawns, numValidSpawners);

                // Shuffle the spawners to get a random selection
                var shuffledSpawners = validSpawners.OrderBy(x => UnityEngine.Random.value).ToArray();

                // Add the first `randNumberSpawns` from the shuffled list to the selected spawners
                for (int j = 0; j < randNumberSpawns; j++)
                {
                    //selectedSpawners.Add(shuffledSpawners[j]);
                    var onDestroySpawn = shuffledSpawners[j].spawnedNetworkObject.AddComponent<OnDestroySpawnNetworkObject>();
                    onDestroySpawn.SpawnPrefab = spawnPrefab;
                    onDestroySpawn.Offset = spawnOffset;
                }


                /*
                int onDestroyAddedCount = 0;

                for (int j = 0; j < numSpawners; j++)
                {
                    // continue if this was a null spawner (note Spawner_NetworkObject_v2's MUST be set
                    // before this code to ensure it works correctly)
                    if (spawners[j].spawnedNetworkObject == null) continue;

                    // check if we are at max adds
                    if (onDestroyAddedCount >= maxNumberSpawns) break;

                    // check if we are at the point where we definitely must add a spawn
                    bool isMustSpawn = (numSpawners - j) <= (minNumberSpawns - onDestroyAddedCount);

                    // get rand spawn
                    var rand = UnityEngine.Random.Range(0f, 1f);

                    // spawn if we need to or we got chance to
                    if (isMustSpawn || rand <= spawnChance)
                    {
                        var onDestroySpawn = spawners[j].spawnedNetworkObject.AddComponent<OnDestroySpawnNetworkObject>();
                        onDestroySpawn.SpawnPrefab = spawnPrefab;
                        onDestroySpawn.Offset = spawnOffset;
                        onDestroyAddedCount++;
                    }
                }

                */
            }



        }
    }
}
