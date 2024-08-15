using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionNameItemViewIntelligenceTab : WinnerStatusNameItemView<DecisionItemViewModelIntelligenceTab>
    {
        public DecisionNameItemViewIntelligenceTab() : base(true, false, true)
        {
        }
        
        protected override void OnRegisterViewModelEvents(DecisionItemViewModelIntelligenceTab viewModel)
        {
            base.OnRegisterViewModelEvents(viewModel);
            
            OnDecisionContextChanged(viewModel.DecisionContextViewModel.Context);
            viewModel.DecisionContextViewModel.ContextChanged += OnDecisionContextChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionItemViewModelIntelligenceTab viewModel)
        {
            base.OnUnregisterViewModelEvents(viewModel);
            
            viewModel.DecisionContextViewModel.ContextChanged -= OnDecisionContextChanged;
        }

        private void OnDecisionContextChanged(DecisionContext context)
        {
            if (!IsRuntime)
                IsWinner = context.IsWinner;
        }
    }
}