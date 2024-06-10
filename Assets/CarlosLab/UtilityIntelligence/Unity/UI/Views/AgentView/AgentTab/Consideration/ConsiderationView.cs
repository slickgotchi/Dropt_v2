#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationView : NameView<ConsiderationViewModel>
    {
        private TabView agentTabView;

        private Button editButton;
        private InputView inputView;

        private ConsiderationEditorListView listView;

        private ResponseCurveView responseCurveView;

        public ConsiderationView() : base(UIBuilderResourcePaths.NameView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            this.titleLabel.text = "Consideration";
            inputView = new InputView();
            inputView.style.marginTop = 5.0f;
            container.Add(inputView);

            responseCurveView = new ResponseCurveView();
            container.Add(responseCurveView);

            editButton = new Button();
            editButton.text = "Edit";
            editButton.style.width = 100;
            editButton.style.marginTop = 20;
            editButton.style.marginLeft = 30;

            editButton.style.alignSelf = Align.Center;
            editButton.clicked += SwitchToEditView;
            container.Add(editButton);
        }

        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            agentTabView = panel.visualTree.Q<TabView>();
            listView = panel.visualTree.Q<ConsiderationEditorListView>();
        }

        private void SwitchToEditView()
        {
            agentTabView.selectedTabIndex = IntelligenceViewTabIndexes.ConsiderationTab;
            listView.SelectedIndex = ViewModel.EditorViewModel.Index;
        }

        protected override void OnUpdateView(ConsiderationViewModel viewModel)
        {
            inputView.UpdateView(viewModel);
            responseCurveView.UpdateView(viewModel);
        }
    }
}