#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class ConsiderationContainerView : BaseView<DecisionViewModel>, IMainView<DecisionSubView>
    {
        private readonly VisualElement container;

        private readonly ConsiderationItemCreatorView itemCreatorView;
        private readonly ConsiderationListView listView;
        
        private readonly Toggle reorderableToggle;

        public ConsiderationContainerView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Considerations";
            Add(foldout);
            
            container = new();
            foldout.Add(container);

            reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.RegisterValueChangedCallback(evt => listView.Reorderable = evt.newValue);
            container.Add(reorderableToggle);

            listView = new();
            listView.style.marginTop = 5;

            container.Add(listView);

            itemCreatorView = new();
            container.Add(itemCreatorView);
        }
        

        public DecisionSubView SubView { get; private set; }

        public void InitSubView(DecisionSubView subView)
        {
            SubView = subView;
            listView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionViewModel viewModel)
        {
            listView.UpdateView(viewModel.Considerations);
            itemCreatorView.UpdateView(viewModel.Considerations);
        }
    }
}