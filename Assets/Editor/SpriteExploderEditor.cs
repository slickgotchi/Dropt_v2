// Custom inspector for SpriteExploder
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteExploder))]
public class SpriteExploderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteExploder exploder = (SpriteExploder)target;
        if (GUILayout.Button("Generate Fragments"))
        {
            exploder.GenerateFragments();
        }
    }
}
