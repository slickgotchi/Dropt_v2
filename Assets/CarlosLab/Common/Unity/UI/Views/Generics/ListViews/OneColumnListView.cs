#region

using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class
        OneColumnListView<TListViewModel, TItemViewModel> : BaseListView<TListViewModel, TItemViewModel, ListView>
        where TListViewModel : class, IListViewModelWithViewModel<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        protected OneColumnListView(string visualAssetPath) : base(visualAssetPath)
        {
        }

        protected sealed override void InitListView(ListView listView)
        {
            listView.makeItem = HandleMakeItem;
            listView.bindItem = HandleBindItem;
            base.InitListView(listView);
        }

        private VisualElement HandleMakeItem()
        {
            return OnMakeItem();
        }

        private void HandleBindItem(VisualElement element, int index)
        {
            if (index >= ViewModel.Items.Count || index < 0)
                return;

            OnBindItem(element, index);
        }

        protected abstract VisualElement OnMakeItem();
        protected abstract void OnBindItem(VisualElement element, int index);
    }
}