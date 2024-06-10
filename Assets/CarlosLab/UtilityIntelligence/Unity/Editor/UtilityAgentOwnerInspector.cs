#region

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    [CustomEditor(typeof(UtilityAgentController))]
    public class UtilityAgentOwnerInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new();

            ObjectField assetField = new();
            assetField.label = "Intelligence Asset";
            assetField.bindingPath = nameof(UtilityAgentController.editorAsset);

            inspector.Add(assetField);

            Button openEditorButton = new();
            openEditorButton.text = "Open Editor";
            openEditorButton.clicked += () =>
            {
                UtilityAgentController controller = (UtilityAgentController)target;
                if(controller.EditorAsset == null)
                {
                    Debug.LogError("Intelligence Asset is null");
                    return;
                }
                
                UtilityIntelligenceEditorWindow.OpenWindow();
            };

            inspector.Add(openEditorButton);

            return inspector;
        }
    }
}