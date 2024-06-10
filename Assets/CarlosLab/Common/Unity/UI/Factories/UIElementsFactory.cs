#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public static class UIElementsFactory
    {
        public static Label CreateTitleLabel(string title = "")
        {
            Label label = new(title);
            label.AddToClassList("title-label");
            return label;
        }
    }
}