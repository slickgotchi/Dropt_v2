namespace CarlosLab.Common.UI
{
    public interface INameListViewModel : IListViewModel
    {
        bool Contains(string name);
    }
}