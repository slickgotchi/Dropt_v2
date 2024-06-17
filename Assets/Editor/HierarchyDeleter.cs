using UnityEngine;
using UnityEditor;

public class HierarchyDeleter : EditorWindow
{
    private GameObject[] selectedObjects;

    [MenuItem("Tools/Hierarchy Deleter")]
    public static void ShowWindow()
    {
        GetWindow<HierarchyDeleter>("Hierarchy Deleter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Delete Selected GameObjects", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Objects"))
        {
            selectedObjects = Selection.gameObjects;
            Debug.Log("Selected " + selectedObjects.Length + " objects.");
        }

        if (selectedObjects != null && selectedObjects.Length > 0)
        {
            if (GUILayout.Button("Delete Selected Objects"))
            {
                DeleteSelectedObjects();
            }
        }
    }

    private void DeleteSelectedObjects()
    {
        foreach (GameObject obj in selectedObjects)
        {
            DestroyImmediate(obj);
        }

        Debug.Log("Deleted " + selectedObjects.Length + " objects.");
        selectedObjects = null;
    }
}
