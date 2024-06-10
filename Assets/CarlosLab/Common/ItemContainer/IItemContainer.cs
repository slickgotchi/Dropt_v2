#region

#endregion

namespace CarlosLab.Common
{
    public interface IItemContainer
    {
        int Count { get; }
        bool Contains(string name);
    }
}