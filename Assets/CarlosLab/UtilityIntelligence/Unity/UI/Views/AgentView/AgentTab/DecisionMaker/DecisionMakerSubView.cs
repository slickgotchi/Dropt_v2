#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerSubView : BaseView
    {
        private readonly DecisionSplitView decisionView;

        public DecisionMakerSubView() : base(null)
        {
            decisionView = new DecisionSplitView();
            decisionView.Show(false);
            Add(decisionView);
        }

        public void ShowDecisionView(DecisionViewModel viewModel)
        {
            decisionView.Show(true);
            decisionView.MainView.UpdateView(viewModel);
        }

        public void HideDecisionView()
        {
            decisionView.Show(false);
        }
    }
}