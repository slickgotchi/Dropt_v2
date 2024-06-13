using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class ClearTilemap
{
    [MenuItem("Tools/Clear All Tiles")]
    private static void ClearAllTiles()
    {
        Tilemap tilemap = Selection.activeGameObject?.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Please select a GameObject with a Tilemap component.");
            return;
        }

        tilemap.ClearAllTiles();
        Debug.Log("All tiles have been cleared from the tilemap.");
    }
}
