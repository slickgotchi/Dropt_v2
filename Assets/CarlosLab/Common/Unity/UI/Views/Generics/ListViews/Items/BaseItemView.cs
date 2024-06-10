#region

using CarlosLab.Common.UI.Extensions;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class BaseItemView<TItemViewModel> : BaseView<TItemViewModel>
        where TItemViewModel : class, IItemViewModel
    {
        protected readonly IListViewWithItem<TItemViewModel> ListView;
        private VisualElement itemContainerView;

        public BaseItemView(IListViewWithItem<TItemViewModel> listView, string visualAssetPath) : base(visualAssetPath)
        {
            ListView = listView;

            style.flexGrow = 1.0f;
        }

        public int Index => ViewModel.Index;

        public VisualElement ItemContainerView
        {
            get
            {
                if (itemContainerView == null)
                {
                    itemContainerView = this.GetFirstAncestorWithClass(BaseListView.itemUssClassName);
                    if (itemContainerView != null)
                    {
                        InitItemContainerView(itemContainerView);
                    }
                }


                return itemContainerView;
            }
        }

        protected virtual void InitItemContainerView(VisualElement itemContainerView)
        {
            
        }

        protected void RemoveFromList()
        {
            ListView.TryRemoveItem(ViewModel);
        }

        protected RemoveButton CreateRemoveButton()
        {
            RemoveButton removeButton = new();
            // deleteButton.style.position = Position.Absolute;
            // deleteButton.style.right = 0;
            removeButton.clicked += () => RemoveFromList();
            Add(removeButton);
            return removeButton;
        }
    }
}