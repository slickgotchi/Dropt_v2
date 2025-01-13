using UnityEditor;
using UnityEngine;

public class TextureCompressionTool : EditorWindow
{
    private enum CompressionQuality { None, Low, Medium, High }
    private CompressionQuality selectedCompression = CompressionQuality.High;
    private int totalTextures = 0;
    private int processedTextures = 0;
    private bool isProcessing = false;

    [MenuItem("Tools/Slick/Set Texture Compression")]
    public static void ShowWindow()
    {
        GetWindow<TextureCompressionTool>("Texture Compression Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Texture Compression Settings", EditorStyles.boldLabel);

        selectedCompression = (CompressionQuality)EditorGUILayout.EnumPopup("Compression Quality:", selectedCompression);

        if (!isProcessing && GUILayout.Button("Set Compression for All Textures"))
        {
            SetTextureCompression(selectedCompression);
        }

        if (isProcessing)
        {
            float progress = (float)processedTextures / totalTextures;
            EditorGUILayout.LabelField("Processing textures...", GUILayout.Height(20));
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), progress, $"{processedTextures}/{totalTextures} textures processed");
        }

        EditorGUILayout.HelpBox("This will apply the selected compression quality to all textures in the project.", MessageType.Info);
    }

    private void SetTextureCompression(CompressionQuality compressionQuality)
    {
        isProcessing = true;
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture");
        totalTextures = textureGuids.Length;
        processedTextures = 0;

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer != null)
            {
                switch (compressionQuality)
                {
                    case CompressionQuality.None:
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        break;
                    case CompressionQuality.Low:
                        importer.textureCompression = TextureImporterCompression.CompressedLQ;
                        break;
                    case CompressionQuality.Medium:
                        importer.textureCompression = TextureImporterCompression.Compressed;
                        break;
                    case CompressionQuality.High:
                        importer.textureCompression = TextureImporterCompression.CompressedHQ;
                        break;
                }

                // Apply changes and reimport the texture
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                processedTextures++;

                // Update the UI
                Repaint();
            }
        }

        isProcessing = false;
        Debug.Log("Finished updating texture compression for all textures.");
    }
}
