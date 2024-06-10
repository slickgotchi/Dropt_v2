#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class IntelligenceTabMainView : NameMainView<UtilityIntelligenceViewModel, IntelligenceTabSubView>
    {
        private Toggle compensationFactorToggle;
        private Toggle momentumBonusToggle;


        private DecisionMakerContainerView decisionMakerContainerView;

        public IntelligenceTabMainView() : base(UIBuilderResourcePaths.UtilityIntelligenceTabMainView)
        {
        }

        protected override void OnVisualAssetLoaded()
        {
            base.OnVisualAssetLoaded();

            compensationFactorToggle = this.Query<Toggle>("CompensationFactorToggle");
            compensationFactorToggle.RegisterValueChangedCallback(evt => ViewModel.EnableCompensationFactor = evt.newValue);
            
            momentumBonusToggle = this.Query<Toggle>("MomentumBonusToggle");
            momentumBonusToggle.RegisterValueChangedCallback(evt => ViewModel.EnableMomentumBonus = evt.newValue);
            
            decisionMakerContainerView = this.Q<DecisionMakerContainerView>();
        }

        protected override void OnInitSubView(IntelligenceTabSubView subView)
        {
            decisionMakerContainerView?.InitSubView(subView);
        }

        protected override void OnUpdateView(UtilityIntelligenceViewModel viewModel)
        {
            compensationFactorToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(UtilityIntelligenceViewModel.EnableCompensationFactor), 
                BindingMode.ToTarget);
            
            momentumBonusToggle.SetDataBinding(
                nameof(Toggle.value), 
                nameof(UtilityIntelligenceViewModel.EnableMomentumBonus), 
                BindingMode.ToTarget);
            
            decisionMakerContainerView.UpdateView(viewModel);
        }
    }
}