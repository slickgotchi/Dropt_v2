using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

[CustomEditor(typeof(GameObjectBrush))]
public class GameObjectBrushEditor : GridBrushEditor
{
    private GameObjectBrush gameObjectBrush { get { return (target as GameObjectBrush); } }

    public override void OnPaintInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabs"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("offset"), true);
        serializedObject.ApplyModifiedProperties();
        Debug.Log("GameObject Brush selected and properties updated.");
    }
}
