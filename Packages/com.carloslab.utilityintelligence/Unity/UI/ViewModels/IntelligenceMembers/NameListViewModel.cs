using CarlosLab.Common;
using CarlosLab.Common.UI;

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameListViewModel<TContainerModel, TItemModel, TItemViewModel> 
        : NameListViewModel<TContainerModel, TItemModel, TItemViewModel, UtilityIntelligenceViewModel>
        where TContainerModel : class, IModel
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : class, IItemViewModelWithModel<TItemModel>, INameViewModel
        , IRootViewModelMember<UtilityIntelligenceViewModel>, new()
    {

    }
}