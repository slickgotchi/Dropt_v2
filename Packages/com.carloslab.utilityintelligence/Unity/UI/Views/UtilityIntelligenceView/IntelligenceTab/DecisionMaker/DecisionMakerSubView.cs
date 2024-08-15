#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerSubView : UtilityIntelligenceViewMember
    {
        private readonly DecisionSplitViewIntelligenceTab decisionView;

        public DecisionMakerSubView()
        {
            decisionView = new DecisionSplitViewIntelligenceTab();
            decisionView.Show(false);
            Add(decisionView);
        }

        public void ShowDecisionView(DecisionItemViewModelIntelligenceTab viewModel)
        {
            decisionView.Show(true);
            decisionView.MainView.UpdateView(viewModel);
        }
        
        public void HideDecisionView()
        {
            decisionView.Show(false);
        }

        protected override void OnRootViewChanged(UtilityIntelligenceView rootView)
        {
            decisionView.RootView = rootView;
        }
    }
}