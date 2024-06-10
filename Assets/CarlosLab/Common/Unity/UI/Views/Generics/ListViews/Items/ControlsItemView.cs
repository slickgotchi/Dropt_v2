#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public class ControlsItemView<TItemViewModel> : BaseItemView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel

    {
        public ControlsItemView(IListViewWithItem<TItemViewModel> listView) : base(listView, null)
        {
            CreateRemoveButton();
            style.alignItems = Align.Center;
        }
    }
}