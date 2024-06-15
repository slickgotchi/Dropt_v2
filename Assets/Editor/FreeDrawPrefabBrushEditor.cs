using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

[CustomEditor(typeof(FreeDrawPrefabBrush))]
public class FreeDrawPrefabBrushEditor : GridBrushEditor
{
    private FreeDrawPrefabBrush freeDrawPrefabBrush { get { return (target as FreeDrawPrefabBrush); } }

    public override void OnPaintInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("offset"), true);
        serializedObject.ApplyModifiedProperties();
    }
}
