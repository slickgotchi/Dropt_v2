#region

using CarlosLab.Common.UI;

#endregion

namespace CarlosLab.UtilityIntelligence.UI
{
    public abstract class MainTypeListView<TListViewModel, TItemViewModel, TSubView> :
        MainBasicListView<TListViewModel, TItemViewModel, TSubView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : BaseItemViewModel
        where TSubView : BaseView
    {
        protected MainTypeListView(bool hasScoreColumn, bool hasTargetColumn) : base(hasScoreColumn, hasTargetColumn)
        {
        }

        protected override string NameColumnTitle => "Type";
    }
}