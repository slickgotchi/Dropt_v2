using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TilemapComponentCopier : EditorWindow
{
    private Tilemap sourceTilemap;
    private Tilemap targetTilemap;

    [MenuItem("Tools/Tilemap Component Copier")]
    public static void ShowWindow()
    {
        GetWindow<TilemapComponentCopier>("Tilemap Component Copier");
    }

    private void OnGUI()
    {
        GUILayout.Label("Copy Tilemap Components", EditorStyles.boldLabel);

        sourceTilemap = (Tilemap)EditorGUILayout.ObjectField("Source Tilemap", sourceTilemap, typeof(Tilemap), true);
        targetTilemap = (Tilemap)EditorGUILayout.ObjectField("Target Tilemap", targetTilemap, typeof(Tilemap), true);

        if (GUILayout.Button("Copy Components"))
        {
            CopyComponents();
        }
    }

    private void CopyComponents()
    {
        if (sourceTilemap == null || targetTilemap == null)
        {
            Debug.LogError("Source or target Tilemap is not assigned.");
            return;
        }

        // Copy all components from source to target
        Component[] components = sourceTilemap.gameObject.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component.GetType() == typeof(Transform) || component.GetType() == typeof(TilemapRenderer) || component.GetType() == typeof(Tilemap))
                continue;

            Component copy = targetTilemap.gameObject.AddComponent(component.GetType());
            System.Reflection.FieldInfo[] fields = component.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(component));
            }
        }

        Debug.Log("Components copied successfully.");
    }
}
