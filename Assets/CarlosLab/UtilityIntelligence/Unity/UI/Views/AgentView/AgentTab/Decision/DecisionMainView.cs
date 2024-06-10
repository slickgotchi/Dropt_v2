#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    [UxmlElement]
    public partial class DecisionMainView : NameMainView<DecisionViewModel, DecisionSubView>
    {
        private ActionContainerView actionContainerView;

        private ConsiderationContainerView considerationContainerView;
        private TargetFilterContainerView targetFilterContainerView;

        private FloatField weightField;

        public DecisionMainView() : base(UIBuilderResourcePaths.DecisionMainView)
        {

        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();
            weightField = this.Q<FloatField>("WeightField");
            weightField.isDelayed = true;
            considerationContainerView = this.Q<ConsiderationContainerView>();
            actionContainerView = this.Q<ActionContainerView>();

            targetFilterContainerView = this.Q<TargetFilterContainerView>();
        }

        protected override void OnInitSubView(DecisionSubView subView)
        {
            considerationContainerView.InitSubView(subView);
            actionContainerView.InitSubView(subView);
            targetFilterContainerView.InitSubView(subView);
        }

        protected override void OnUpdateView(DecisionViewModel viewModel)
        {
            weightField.dataSource = viewModel.ContextViewModel;
            weightField.SetDataBinding(nameof(FloatField.value), nameof(DecisionRuntimeContextViewModel.Weight),
                BindingMode.TwoWay);
            
            actionContainerView.UpdateView(viewModel);
            targetFilterContainerView.UpdateView(viewModel);
            considerationContainerView.UpdateView(viewModel);
        }
    }
}