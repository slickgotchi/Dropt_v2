using UnityEditor;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Editor
{
    public static class UtilityIntelligenceHierarchyCreatorMenu
    {
        [MenuItem(FrameworkEditorConsts.CreateWorldMenuPath, false, 0)]
        static void CreateUtilityWorld(MenuCommand menuCommand)
        {
            GameObject go = new ("Utility World");
            go.AddComponent<UtilityWorldController>();
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
    }
}