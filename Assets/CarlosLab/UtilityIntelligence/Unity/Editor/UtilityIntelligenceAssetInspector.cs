#region

using UnityEditor;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.Editor
{
    [CustomEditor(typeof(UtilityIntelligenceAsset))]
    public class UtilityIntelligenceAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement inspector = new();
            inspector.dataSource = serializedObject.targetObject;

            TextField agentTypeField = new();

            agentTypeField.textEdition.placeholder = "Type...";
            agentTypeField.bindingPath = nameof(UtilityIntelligenceAsset.agentType);

            inspector.Add(agentTypeField);

            TextField agentDescriptionField = new();
            agentDescriptionField.textEdition.placeholder = "Description...";
            agentDescriptionField.bindingPath = nameof(UtilityIntelligenceAsset.agentDescription);
            inspector.Add(agentDescriptionField);


            Button openEditorButton = new();
            openEditorButton.text = "Open Editor";
            openEditorButton.clicked += UtilityIntelligenceEditorWindow.OpenWindow;

            inspector.Add(openEditorButton);

            return inspector;
        }
    }
}