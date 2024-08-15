using System;
using CarlosLab.Common;
using CarlosLab.Common.UI;


namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BaseItemViewModel<TItemModel, TListViewModel> : BaseItemViewModel<TItemModel, TListViewModel, UtilityIntelligenceViewModel>
        where TItemModel : class, IModelWithId
        where TListViewModel : class, IListViewModel
    {
    }
}