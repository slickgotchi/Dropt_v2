#region

using CarlosLab.Common;
using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class StatusTypeNameItemView<TItemViewModel> : StatusBaseNameItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel, ITypeNameViewModel, IStatusViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {
        protected StatusTypeNameItemView(bool enableStatus, bool enableRemove = true) : base( enableStatus, false, enableRemove)
        {
        }

        protected override void OnRefreshView(TItemViewModel viewModel)
        {
            NameLabel.text = viewModel.TypeName;
        }
    }
}