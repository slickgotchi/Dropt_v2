namespace CarlosLab.Common.UI
{
    internal interface INameListViewModel<TItemViewModel> : INameListViewModel,
        IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : IItemViewModel, INameViewModel
    {
    }
}