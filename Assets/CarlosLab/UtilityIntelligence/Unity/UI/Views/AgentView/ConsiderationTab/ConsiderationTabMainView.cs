#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class ConsiderationTabMainView : MainView<ConsiderationEditorListViewModel, ConsiderationTabSubView>
    {
        private readonly ConsiderationEditorItemCreatorView creatorView;
        private readonly ConsiderationEditorListView listView;
        private readonly Toggle reorderableToggle;

        public ConsiderationTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Considerations");
            Add(titleLabel);

            ScrollView scrollView = new();
            Add(scrollView);

            reorderableToggle = new Toggle("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);

            listView = new ConsiderationEditorListView();
            listView.style.marginTop = 5;
            scrollView.Add(listView);

            creatorView = new ConsiderationEditorItemCreatorView();
            // creatorView.style.marginLeft = 10;
            scrollView.Add(creatorView);
        }

        protected override void OnInitSubView(ConsiderationTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(ConsiderationEditorListViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            creatorView.UpdateView(viewModel);
        }
    }
}