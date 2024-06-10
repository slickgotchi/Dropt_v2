#region

using System;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class NameListViewModel<TContainerModel, TItemModel, TItemViewModel> :
        BaseListViewModel<TContainerModel, TItemModel, TItemViewModel>, INameListViewModel<TItemViewModel>
        where TContainerModel : class
        where TItemModel : class, IModelWithId, IContainerItem
        where TItemViewModel : BaseItemViewModel<TItemModel>, INameViewModel
    {
        protected NameListViewModel(IDataAsset asset, TContainerModel model) : base(asset, model)
        {
        }

        public abstract bool Contains(string name);

        public override TItemViewModel CreateItem(Type modelType, object itemInfo)
        {
            string name = itemInfo as string;
            if (Contains(name))
                return null;

            TItemModel model = CreateModel(modelType);
            model.Name = name;

            TryCreateItem(model, out TItemViewModel item);
            return item;
        }

        public override bool TryRenameItem(TItemViewModel item, string newName)
        {
            if (!Contains(newName))
            {
                string oldName = item.Name;
                item.Name = newName;
                if (TryRemoveModelWithoutRecord(oldName, item.Model) &&
                    TryAddModelWithoutRecord(item.Model, item.Index)) return true;
            }

            return false;
        }

        public bool TryRemoveItem(string name)
        {
            TItemViewModel item = items.Find(x => x.Name == name);
            return TryRemoveItem(item);
        }

        public TItemViewModel GetItemByName(string name)
        {
            return items.Find(x => x.Name == name);
        }

        protected abstract bool TryRemoveModelWithoutRecord(string name, TItemModel model);
    }
}