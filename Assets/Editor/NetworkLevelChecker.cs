using UnityEngine;
using UnityEditor;
using Level;
using Unity.Netcode;

public class NetworkLevelChecker : MonoBehaviour
{
    [MenuItem("Tools/Check Accidental Network Objects in NetworkLevel Prefabs")]
    private static void CheckNetworkObjectsInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab.GetComponent<NetworkLevel>() != null)
            {
                //Debug.Log($"Prefab '{prefab.name}' contains a NetworkLevel script.");

                NetworkObject[] networkObjects = prefab.GetComponentsInChildren<NetworkObject>(true);
                foreach (NetworkObject networkObject in networkObjects)
                {
                    if (networkObject.GetComponent<NetworkLevel>() == null)
                    {
                        Debug.Log($"Object '{networkObject.gameObject.name}' in prefab '{prefab.name}' contains a NetworkObject component.");
                    }
                }
            }
        }

        Debug.Log("Finished checking all prefabs.");
    }
}
