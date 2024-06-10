using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterView : NameView<TargetFilterViewModel>
    {
        private Button editButton;
        
        private TabView agentTabView;
        private TargetFilterEditorListView listView;


        public TargetFilterView() : base(UIBuilderResourcePaths.NameView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            this.titleLabel.text = "Target Filter";
            
            editButton = new Button
            {
                text = "Edit",
                style =
                {
                    width = 100,
                    marginTop = 20,
                    marginLeft = 30,
                    alignSelf = Align.Center
                }
            };

            editButton.clicked += SwitchToEditView;
            container.Add(editButton);
        }

        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            agentTabView = panel.visualTree.Q<TabView>();
            listView = panel.visualTree.Q<TargetFilterEditorListView>();
        }

        private void SwitchToEditView()
        {
            agentTabView.selectedTabIndex = IntelligenceViewTabIndexes.TargetFilterTab;
            listView.SelectedIndex = ViewModel.EditorViewModel.Index;
        }
    }
}