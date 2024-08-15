#region

#endregion

using System.Collections.Generic;

namespace CarlosLab.Common
{
    public interface IItemContainer
    {
        int Count { get; }
        bool Contains(string name);
    }
}