using System.Collections.Generic;
using System.Linq;
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
                TrapsGroup trapsGroup = new TrapsGroup();
                
                var maxGroup = group.Spawners.Max(temp => temp.Group);

                foreach (var spawner in group.Spawners)
                {
                    var trap = Object.Instantiate(spawner.Prefab, group.transform);
                    trap.transform.position = spawner.transform.position;
                    trap.GetComponent<NetworkObject>().Spawn();
                    trap.GetComponent<NetworkObject>().TrySetParent(parent);
                    trap.GetComponent<Trap>().SetupGroup(trapsGroup, spawner.Group, maxGroup);

                    trapsGroup.Traps.Add(trap.GetComponent<Trap>());
                }
                
                CleanupFactory.DestroyAllChildren(group.transform);
            }
        }
    }
}