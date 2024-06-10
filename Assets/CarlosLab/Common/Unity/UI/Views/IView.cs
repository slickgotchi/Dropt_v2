#region

using System;

#endregion

namespace CarlosLab.Common.UI
{
    public interface IView
    {
        event Action Shown;
        event Action Hidden;
        void Show(bool show);
    }
}