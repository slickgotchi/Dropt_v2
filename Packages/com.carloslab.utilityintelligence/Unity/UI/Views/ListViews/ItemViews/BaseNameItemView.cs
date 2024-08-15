using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BaseNameItemView<TItemViewModel> : BaseNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        public BaseNameItemView(bool enableRename, bool enableRemove = true) : base(enableRename, enableRemove)
        {
        }
    }
}