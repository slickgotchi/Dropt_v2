using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public class TypeNameItemView<TItemViewModel>
        : TypeNameItemView<TItemViewModel, UtilityIntelligenceView>
        where TItemViewModel : class, IItemViewModel, ITypeNameViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>

    {

    }
}