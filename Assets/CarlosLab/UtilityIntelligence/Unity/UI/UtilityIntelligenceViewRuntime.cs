#region

using CarlosLab.Common.UI;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(UtilityAgentController))]
    public class UtilityIntelligenceViewRuntime : MonoBehaviour
    {
        private UtilityAgentController agentController;
        private VisualElement root;
        private UIDocument uiDocument;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            root = uiDocument.rootVisualElement;
            agentController = GetComponent<UtilityAgentController>();
        }

        private void Start()
        {
            UtilityIntelligenceView intelligenceView = new();
            root.Add(intelligenceView);
            UtilityIntelligenceAsset asset = agentController.Asset;

            if (asset == null || asset.Model == null)
                return;

            UtilityIntelligenceViewModel viewModel = ViewModelFactory<UtilityIntelligenceViewModel>.Create(asset, asset.Model);
            intelligenceView.UpdateView(viewModel);
        }
    }
}