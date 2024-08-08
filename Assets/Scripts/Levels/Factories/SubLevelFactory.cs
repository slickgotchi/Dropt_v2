using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Level
{
    public static class SubLevelFactory
    {
        public static void CreateSubLevels(GameObject parent)
        {
            var subLevelSpawners = new List<SubLevelSpawner>(parent.GetComponentsInChildren<SubLevelSpawner>());

            foreach (var subLevelSpawner in subLevelSpawners)
            {
                NormalizeSubLevelSpawner(subLevelSpawner);

                var subLevelPrefab = GetRandomSubLevel(subLevelSpawner);
                if (subLevelPrefab != null)
                {
                    //LevelManager.Instance.LevelSpawningCount++;

                    var newSubLevel = Object.Instantiate(subLevelPrefab);
                    newSubLevel.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        private static void NormalizeSubLevelSpawner(SubLevelSpawner subLevelSpawner)
        {
            float sum = 0;
            foreach (var subLevel in subLevelSpawner.SubLevels)
            {
                sum += subLevel.SpawnChance;
            }

            foreach (var subLevel in subLevelSpawner.SubLevels)
            {
                subLevel.SpawnChance /= sum;
            }
        }

        private static GameObject GetRandomSubLevel(SubLevelSpawner subLevelSpawner)
        {
            var randValue = UnityEngine.Random.Range(0, 0.999f);
            foreach (var subLevel in subLevelSpawner.SubLevels)
            {
                if (randValue < subLevel.SpawnChance)
                {
                    return subLevel.SubLevelPrefab;
                }
                else
                {
                    randValue -= subLevel.SpawnChance;
                }
            }
            return null;
        }
    }
}
