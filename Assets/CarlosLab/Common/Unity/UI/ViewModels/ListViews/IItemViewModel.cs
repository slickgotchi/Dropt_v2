namespace CarlosLab.Common.UI
{
    public interface IItemViewModel : IViewModel
    {
        string ModelId { get; }
        int Index { get; }
        bool IsInList { get; }
        IListViewModel BaseListViewModel { get; }
        void HandleItemAdded(IListViewModel listViewModel);
        void HandleItemRemoved();
    }
}