using System.Collections.Generic;
using Level.Traps;
using Unity.Netcode;
using UnityEngine;

namespace Level
{
    public static class TrapsGroupSpawnerFactory
    {
        public static void CreateTraps(GameObject parent)
        {
            var groups = new List<TrapsGroupSpawner>(parent.GetComponentsInChildren<TrapsGroupSpawner>());

            foreach (var group in groups)
            {
                foreach (var spawner in group.Spawners)
                {
                    var trap = Object.Instantiate(spawner.Prefab, group.transform);
                    trap.transform.position = spawner.transform.position;
                    trap.GetComponent<NetworkObject>().Spawn();
                    trap.GetComponent<NetworkObject>().TrySetParent(parent);
                    trap.GetComponent<Trap>().SetupGroup(group, spawner.Group);
                }
                
                CleanupFactory.DestroyAllChildren(group.transform);
            }
        }
    }
}