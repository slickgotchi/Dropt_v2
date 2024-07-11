using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public static class CleanupFactory
    {
        public static void DestroySpawnerObjects<T>(GameObject parent) where T : Component
        {
            var spawnerObjects = new List<T>(parent.GetComponentsInChildren<T>());
            foreach (var spawnerObject in spawnerObjects)
            {
                Object.Destroy(spawnerObject.gameObject);
            }
        }

        public static void DestroyAllChildren(Transform parent)
        {
            while (parent.childCount > 0)
            {
                var child = parent.GetChild(0);
                child.parent = null;
                Object.Destroy(child.gameObject);
            }
        }
    }
}
