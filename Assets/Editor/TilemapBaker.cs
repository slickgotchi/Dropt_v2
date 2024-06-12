using Tilemaps.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBakerEditor : EditorWindow
{
    private Tilemap sourceTilemap;
    private Tilemap targetTilemap;

    [MenuItem("Tools/Tilemap Baker")]
    public static void ShowWindow()
    {
        GetWindow<TilemapBakerEditor>("Tilemap Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tilemap Baker", EditorStyles.boldLabel);

        sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", sourceTilemap, typeof(Tilemap), true);
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", targetTilemap, typeof(Tilemap), true);

        if (GUILayout.Button("Bake Tilemap"))
        {
            if (sourceTilemap != null && targetTilemap != null)
            {
                BakeTilemap();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign both source and target tilemaps.", "OK");
            }
        }
    }

    private void BakeTilemap()
    {
        Undo.RecordObject(targetTilemap, "Bake Tilemap");

        // Clear the target tilemap
        if (sourceTilemap != targetTilemap)
        {
            targetTilemap.ClearAllTiles();
        }

        TileMapTreeGeneratorEditor.SwitchToStatic(sourceTilemap, targetTilemap);

        EditorUtility.SetDirty(targetTilemap);
        Debug.Log("Tilemap baking complete.");
    }
}