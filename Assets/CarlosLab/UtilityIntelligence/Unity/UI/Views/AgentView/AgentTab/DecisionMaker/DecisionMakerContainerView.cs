#region

using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMakerContainerView : BaseView<UtilityIntelligenceViewModel>, IMainView<IntelligenceTabSubView>
    {
        private readonly DecisionMakerItemCreatorView itemCreatorView;
        private readonly DecisionMakerListView listView;
        private readonly Toggle reorderableToggle;

        public DecisionMakerContainerView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Decision Makers";
            Add(foldout);

            reorderableToggle = new Toggle("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;

            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            foldout.Add(reorderableToggle);

            listView = new DecisionMakerListView();
            listView.style.marginTop = 5;
            foldout.Add(listView);

            itemCreatorView = new DecisionMakerItemCreatorView();
            foldout.Add(itemCreatorView);
        }

        public IntelligenceTabSubView SubView { get; private set; }

        public void InitSubView(IntelligenceTabSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(UtilityIntelligenceViewModel viewModel)
        {
            listView.UpdateView(viewModel.DecisionMakers);
            itemCreatorView.UpdateView(viewModel.DecisionMakers);
        }
    }
}