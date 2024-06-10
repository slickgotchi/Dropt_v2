using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TargetFilterTabMainView : MainView<TargetFilterEditorListViewModel, TargetFilterTabSubView>
    {
        private readonly TargetFilterEditorItemCreatorView creatorView;
        private readonly TargetFilterEditorListView listView;
        private readonly Toggle reorderableToggle;

        public TargetFilterTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("TargetFilters");
            Add(titleLabel);

            ScrollView scrollView = new();
            Add(scrollView);

            reorderableToggle = new Toggle("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new TargetFilterEditorListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            creatorView = new TargetFilterEditorItemCreatorView();
            // creatorView.style.marginLeft = 10;
            scrollView.Add(creatorView);
        }

        protected override void OnInitSubView(TargetFilterTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(TargetFilterEditorListViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            creatorView.UpdateView(viewModel);
        }
    }
}