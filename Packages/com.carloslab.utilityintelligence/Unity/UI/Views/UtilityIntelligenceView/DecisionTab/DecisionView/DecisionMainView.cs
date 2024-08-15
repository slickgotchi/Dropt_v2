#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMainView : NameMainView<DecisionItemViewModel, DecisionSubView>
    {
        private FloatField weightField;

        private Toggle hasNoTargetToggle;
        private Toggle enableCachePerTargetToggle;

        private TargetFilterContainerViewDecisionTab targetFilterContainerView;
        private ActionContainerViewDecisionTab actionContainerView;
        private ConsiderationContainerViewDecisionTab considerationContainerView;



        public DecisionMainView() : base(UIBuilderResourcePaths.DecisionMainView)
        {

        }

        protected override void OnLoadVisualAssetSuccess()
        {
            base.OnLoadVisualAssetSuccess();
            
            weightField = this.Q<FloatField>("WeightField");
            weightField.isDelayed = true;
            weightField.RegisterValueChangedCallback(evt =>
            {
                var newValue = evt.newValue;
                var oldValue = evt.previousValue;
                if (newValue < 0.0f)
                {
                    weightField.SetValueWithoutNotify(oldValue);
                }
                else
                {
                    ViewModel.Weight = newValue;
                }
            });
            
            hasNoTargetToggle = this.Q<Toggle>("HasNoTargetToggle");
            hasNoTargetToggle.RegisterValueChangedCallback(evt =>
            {
                bool hasNoTarget = evt.newValue;
                ViewModel.HasNoTarget = hasNoTarget;
                enableCachePerTargetToggle.SetDisplay(!hasNoTarget);
                targetFilterContainerView.SetDisplay(!hasNoTarget);
            });
            
            enableCachePerTargetToggle = this.Q<Toggle>("EnableCachePerTargetToggle");
            enableCachePerTargetToggle.RegisterValueChangedCallback(evt =>
            {
                ViewModel.EnableCachePerTarget = evt.newValue;
            });
            
            considerationContainerView = this.Q<ConsiderationContainerViewDecisionTab>();
            actionContainerView = this.Q<ActionContainerViewDecisionTab>();

            targetFilterContainerView = this.Q<TargetFilterContainerViewDecisionTab>();
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            considerationContainerView.InitSubView(subView);
            actionContainerView.InitSubView(subView);
            targetFilterContainerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionItemViewModel viewModel)
        {
            actionContainerView.UpdateView(viewModel);
            targetFilterContainerView.UpdateView(viewModel);
            considerationContainerView.UpdateView(viewModel?.ConsiderationListViewModel);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            actionContainerView.RootView = rootView;
            targetFilterContainerView.RootView = rootView;
            considerationContainerView.RootView = rootView;
        }

        protected override void OnRefreshView(DecisionItemViewModel viewModel)
        {
            base.OnRefreshView(viewModel);
            weightField.SetDataBinding(nameof(FloatField.value), nameof(DecisionItemViewModel.Weight),
                BindingMode.ToTarget);
            
            hasNoTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(DecisionItemViewModel.HasNoTarget), 
                BindingMode.ToTarget);
            
            enableCachePerTargetToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(DecisionItemViewModel.EnableCachePerTarget), 
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            base.OnResetView();
            weightField.ClearBindings();
            hasNoTargetToggle.ClearBindings();
        }
    }
}