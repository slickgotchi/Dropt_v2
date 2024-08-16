using System.Collections.Generic;
using Level;
using Microsoft.IdentityModel.Tokens;
using UnityEngine;

namespace Chest
{
    public static class ChestsSpawnerFactory
    {
        public static GameObject CreateChests(GameObject source)
        {
            var groups = new List<ChestsGroupSpawner>(source.GetComponentsInChildren<ChestsGroupSpawner>());

            if (groups.IsNullOrEmpty())
            {
                return null;
            }

            foreach (var group in groups)
            {
                var parent = group.transform;

                foreach (var spawner in group.Spawners)
                {
                    var spawnerTransform = spawner.transform;

                    var position = spawnerTransform.position;
                    var rotation = spawnerTransform.rotation;

                    var chest = Object.Instantiate(spawner.Prefab, position, rotation, parent);
                    chest.SetUp(spawner.Config);
                    chest.NetworkObject.Spawn();

                    CleanupFactory.DestroyAllChildren(parent);
                }
            }

            return null;
        }
    }
}