#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerMainView : NameMainView<DecisionMakerViewModel, DecisionMakerSubView>
    {
        private DecisionItemCreatorView itemCreatorView;
        private DecisionListView listView;
        private Toggle reorderableToggle;

        public DecisionMakerMainView() : base(UIBuilderResourcePaths.DecisionMakerMainView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            
            reorderableToggle = this.Q<Toggle>("ReorderableToggle");
            
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);

            listView = this.Q<DecisionListView>();
            itemCreatorView = this.Q<DecisionItemCreatorView>();
        }

        protected override void OnInitSubView(DecisionMakerSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionMakerViewModel viewModel)
        {
            listView.UpdateView(viewModel.Decisions);
            itemCreatorView.UpdateView(viewModel.Decisions);
        }
    }
}