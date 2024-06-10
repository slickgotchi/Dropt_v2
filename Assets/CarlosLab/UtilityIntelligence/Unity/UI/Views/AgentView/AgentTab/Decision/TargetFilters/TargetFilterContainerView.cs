#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class TargetFilterContainerView : BaseView<DecisionViewModel>, IMainView<DecisionSubView>
    {
        private readonly VisualElement container;
        
        private readonly Toggle hasNoTargetToggle;
        private readonly Toggle reorderableToggle;
        
        private readonly TargetFilterListView listView;
        private readonly TargetFilterItemCreatorView itemCreatorView;

        public TargetFilterContainerView() : base(null)
        {
            Foldout foldout = new();
            foldout.text = "Target Filters";
            Add(foldout);
            
            hasNoTargetToggle = new("Has No Target");
            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
                container.SetDisplay(!hasNoTarget);
            });
            foldout.Add(hasNoTargetToggle);
            
            container = new();
            foldout.Add(container);
            
            reorderableToggle = new("Reorderable");
            reorderableToggle.tooltip = ToolTips.Reorderable;
            reorderableToggle.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            reorderableToggle.style.borderTopWidth = 1.0f;
            reorderableToggle.style.paddingTop = 5;
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
            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(DecisionViewModel.HasNoTarget), 
                BindingMode.ToTarget);
            
            listView.UpdateView(viewModel.TargetFilters);
            itemCreatorView.UpdateView(viewModel.TargetFilters);
        }
    }
}