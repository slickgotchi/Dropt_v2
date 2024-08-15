#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerBestDecisionItemView : BaseNameItemView<DecisionMakerItemViewModel>
    {
        public DecisionMakerBestDecisionItemView() : base( false)
        {
        }

        protected override void OnRefreshView(DecisionMakerItemViewModel viewModel)
        {
            NameLabel.dataSource = viewModel.ContextViewModel;
            NameLabel.SetDataBinding(nameof(Label.text), nameof(DecisionMakerContextViewModel.BestDecisionName),
                BindingMode.ToTarget);
        }

        protected override void OnResetView()
        {
            NameLabel.ClearBindings();
        }
    }
}