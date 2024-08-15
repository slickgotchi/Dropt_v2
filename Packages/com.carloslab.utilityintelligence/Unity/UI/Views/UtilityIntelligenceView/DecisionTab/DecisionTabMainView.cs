using CarlosLab.Common.UI;
using UnityEngine.UIElements;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionTabMainView : MainView<DecisionListViewModel, DecisionTabSubView>
    {
        private DecisionItemCreatorView itemCreatorView;
        private DecisionListView listView;
        public DecisionListView ListView => listView;

        public DecisionTabMainView() : base(null)
        {
            Label titleLabel = UIElementsFactory.CreateTitleLabel("Decisions");
            Add(titleLabel);
            
            ScrollView scrollView = new();
            Add(scrollView);
            
            Toggle reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.marginTop = 5;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            scrollView.Add(reorderableToggle);
            
            listView = new();
            listView.style.marginTop = 5;
            scrollView.Add(listView);
            
            itemCreatorView = new();
            // creatorView.style.marginLeft = 10;
            scrollView.Add(itemCreatorView);
        }
        
        protected override void OnInitSubView(DecisionTabSubView subView)
        {
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionListViewModel viewModel)
        {
            listView.UpdateView(viewModel);
            itemCreatorView.UpdateView(viewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            listView.RootView = rootView;
            itemCreatorView.RootView = rootView;
        }
    }
}