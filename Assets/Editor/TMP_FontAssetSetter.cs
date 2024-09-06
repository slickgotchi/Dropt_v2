using UnityEngine;
using UnityEditor;
using TMPro;

public class TMP_FontAssetSetter : EditorWindow
{
    // Font asset to be set
    public TMP_FontAsset fontAsset;

    // Add the script to the Tools menu
    [MenuItem("Tools/Set TextMeshPro Font Asset")]
    public static void ShowWindow()
    {
        // Create the editor window
        GetWindow<TMP_FontAssetSetter>("Set TMP Font Asset");
    }

    // Display the window interface
    void OnGUI()
    {
        GUILayout.Label("Set Font Asset for all TextMeshPro Components", EditorStyles.boldLabel);

        // Drag and drop TMP_FontAsset
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);

        if (fontAsset == null)
        {
            EditorGUILayout.HelpBox("Please assign a TMP_FontAsset to set.", MessageType.Warning);
        }

        // Button to execute the search and replace
        if (GUILayout.Button("Set Font Asset") && fontAsset != null)
        {
            SetFontAssetForAllTextMeshProComponents();
        }
    }

    // Method to set the font asset for all TextMeshPro components in the project
    void SetFontAssetForAllTextMeshProComponents()
    {
        // Find all TMP_Text components in the project
        string[] allAssetGuids = AssetDatabase.FindAssets("t:GameObject");

        foreach (string guid in allAssetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (gameObject != null)
            {
                // Find all TextMeshPro components on the GameObject
                TMP_Text[] textComponents = gameObject.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text tmp in textComponents)
                {
                    Undo.RecordObject(tmp, "Set TMP Font Asset"); // Allow for undo functionality
                    tmp.font = fontAsset;
                    EditorUtility.SetDirty(tmp); // Mark the component as dirty so the change is saved
                }
            }
        }

        // Save the changes made to the assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Font asset has been set for all TextMeshPro components.");
    }
}
