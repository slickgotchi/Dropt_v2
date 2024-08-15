#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class NameValueListView<TListViewModel, TItemViewModel> :
        NameValueListView<TListViewModel, TItemViewModel, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, INameViewModel, IValueViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {

    }
}