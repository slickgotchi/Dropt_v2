#region

using CarlosLab.Common;
using CarlosLab.Common.UI;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class BasicListView<TListViewModel, TItemViewModel> :
        BasicListView<TListViewModel, TItemViewModel, UtilityIntelligenceView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>, IRootViewModelMember<UtilityIntelligenceViewModel>
        where TItemViewModel : class, IItemViewModel, IRootViewModelMember<UtilityIntelligenceViewModel>
    {
        protected BasicListView(string visualAssetPath) : base(visualAssetPath)
        {
        }
    }
}