#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class IntelligenceTabSubView : BaseView
    {
        private readonly DecisionMakerSplitView decisionMakerView;

        public IntelligenceTabSubView() : base(null)
        {
            decisionMakerView = new DecisionMakerSplitView();
            decisionMakerView.Show(false);

            Add(decisionMakerView);
        }

        public void ShowDecisionMakerView(DecisionMakerViewModel viewModel)
        {
            decisionMakerView.Show(true);
            decisionMakerView.MainView.UpdateView(viewModel);
        }

        public void HideDecisionMakerView()
        {
            decisionMakerView.Show(false);
        }
    }
}