using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class CompositeTilemapColliderChecker : EditorWindow
{
    [MenuItem("Tools/Slick/Check Tilemap Colliders")]
    public static void ShowWindow()
    {
        GetWindow<CompositeTilemapColliderChecker>("Check Tilemap Colliders").Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Check Prefabs for TilemapCollider2D & CompositeCollider2D", EditorStyles.boldLabel);

        if (GUILayout.Button("Check and Fix Prefabs"))
        {
            CheckAndFixCollidersInPrefabs();
        }
    }

    private static void CheckAndFixCollidersInPrefabs()
    {
        // Find all prefab assets in the project
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in allPrefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                CheckAndFixCollidersInGameObject(prefab);
            }
        }

        // Save any modified assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Completed checking all prefabs.");
    }

    private static void CheckAndFixCollidersInGameObject(GameObject gameObject)
    {
        // Get all TilemapCollider2D components in the prefab
        TilemapCollider2D[] tilemapColliders = gameObject.GetComponentsInChildren<TilemapCollider2D>(true);

        foreach (var tilemapCollider in tilemapColliders)
        {
            // Check if the GameObject also has a CompositeCollider2D
            CompositeCollider2D compositeCollider = tilemapCollider.GetComponent<CompositeCollider2D>();

            if (compositeCollider != null)
            {
                // Ensure "Used by Composite" is ticked
                if (!tilemapCollider.usedByComposite)
                {
                    tilemapCollider.usedByComposite = true;
                    EditorUtility.SetDirty(tilemapCollider); // Mark as dirty for saving
                    Debug.Log($"Updated 'Used by Composite' for {gameObject.name} at {AssetDatabase.GetAssetPath(gameObject)}");
                }
            }
        }
    }
}
