#region

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerNameItemView : WinnerStatusNameItemView<DecisionMakerItemViewModel>
    {
        public DecisionMakerNameItemView() : base(true, true, true)
        {
        }

        protected override void OnRegisterViewModelEvents(DecisionMakerItemViewModel viewModel)
        {
            base.OnRegisterViewModelEvents(viewModel);

            OnContextChanged(viewModel.ContextViewModel.Context);
            viewModel.ContextViewModel.ContextChanged += OnContextChanged;
        }

        protected override void OnUnregisterViewModelEvents(DecisionMakerItemViewModel viewModel)
        {
            base.OnUnregisterViewModelEvents(viewModel);
            
            viewModel.ContextViewModel.ContextChanged -= OnContextChanged;
        }

        private void OnContextChanged(DecisionMakerContext context)
        {
            if (!IsRuntime)
                IsWinner = context.IsWinner;
        }
    }
}