#region

using CarlosLab.Common.UI;
using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public class DecisionMakerBestDecisionItemView : BaseNameItemView<DecisionMakerViewModel>
    {
        public DecisionMakerBestDecisionItemView(IListViewWithItem<DecisionMakerViewModel> listView) : base(false,
            listView)
        {
        }

        protected override void OnUpdateView(DecisionMakerViewModel viewModel)
        {
            NameLabel.SetDataBinding(nameof(Label.text), nameof(DecisionMakerViewModel.BestDecisionName),
                BindingMode.ToTarget);
        }
    }
}