using System;
using System.Collections.Generic;

namespace CarlosLab.Common.UI
{
    public abstract class BaseListViewModel<TItemModel, TItemViewModel> : BaseListViewModel<TItemViewModel>,
        IListViewModelWithModel<TItemModel>
        where TItemModel : class, IModelWithId
        where TItemViewModel : BaseItemViewModel<TItemModel>
    {
        private readonly Dictionary<string, TItemViewModel> itemDict = new();

        protected BaseListViewModel(IDataAsset asset, object model) : base(asset, model)
        {
        }

        public abstract IReadOnlyList<TItemModel> ItemModels { get; }

        #region Create Items

        public override TItemViewModel CreateItem(Type modelType, object itemInfo)
        {
            TItemModel model = CreateModel(modelType);
            TryCreateItem(model, out TItemViewModel item);
            return item;
        }

        public bool TryCreateItem(TItemModel model, out TItemViewModel newItem)
        {
            newItem = null;

            TItemViewModel viewModel = CreateViewModel(model);
            if (TryAddItem(viewModel))
            {
                newItem = viewModel;
                return true;
            }

            return false;
        }

        protected bool TryCreateItemWithoutModel(TItemModel model, out TItemViewModel item)
        {
            return TryCreateItemWithoutModel(model, items.Count, out item);
        }

        protected bool TryCreateItemWithoutModel(TItemModel model, int index, out TItemViewModel newItem,
            bool notify = false)
        {
            newItem = null;

            TItemViewModel viewModel = CreateViewModel(model);
            if (TryAddItemWithoutModel(viewModel, index, notify))
            {
                newItem = viewModel;
                return true;
            }

            return false;
        }

        protected virtual TItemViewModel CreateViewModel(TItemModel model)
        {
            return ViewModelFactory<TItemViewModel>.Create(Asset, model);
        }

        private bool TryAddModel(TItemModel model)
        {
            bool result = false;
            Record($"Add Item: {typeof(TItemModel).Name}",
                () => { result = TryAddModelWithoutRecord(model); });

            return result;
        }

        protected virtual TItemModel CreateModel(Type runtimeType)
        {
            return ModelFactory<TItemModel>.Create(runtimeType);
        }
        
        private bool TryAddItemWithoutModel(TItemViewModel item, bool notify = false)
        {
            return TryAddItemWithoutModel(item, items.Count, notify);
        }

        private bool TryAddItemWithoutModel(TItemViewModel item, int index, bool notify = false)
        {
            if (item == null)
                return false;

            if (itemDict.TryAdd(item.ModelId, item))
            {
                items.Insert(index, item);
                HandleItemAdded(item);

                if (notify)
                    RaiseItemAdded(item);
                return true;
            }

            return false;
        }

        public override bool TryAddItem(TItemViewModel item)
        {
            if (item == null)
                return false;

            if (TryAddModel(item.Model) && TryAddItemWithoutModel(item, true))
                return true;

            return false;
        }

        private void HandleItemAdded(TItemViewModel item)
        {
            item.HandleItemAdded(this);
            OnItemAdded(item);
        }

        protected virtual void OnItemAdded(TItemViewModel item)
        {
        }

        private bool TryAddModelWithoutRecord(TItemModel model)
        {
            return TryAddModelWithoutRecord(model, ItemModels.Count);
        }


        protected abstract bool TryAddModelWithoutRecord(TItemModel model, int index);

        #endregion

        #region Remove Items

        public override bool TryRemoveItem(TItemViewModel item)
        {
            if (item == null)
                return false;

            if (TryRemoveModel(item.Model) && TryRemoveItemWithoutModel(item, true))
                return true;
            return false;
        }

        protected bool TryRemoveItemWithoutModel(TItemViewModel item, bool notify = false)
        {
            if (item == null)
                return false;

            if (itemDict.Remove(item.ModelId))
            {
                if (notify)
                    RaiseItemRemoving(item);

                items.Remove(item);
                HandleItemRemoved(item);

                if (notify)
                    RaiseItemRemoved(item);

                return true;
            }

            return false;
        }

        private bool TryRemoveModel(TItemModel item)
        {
            bool result = false;

            Record($"Remove Item: {typeof(TItemModel).Name}",
                () => { result = TryRemoveModelWithoutRecord(item); });

            return result;
        }

        private void HandleItemRemoved(TItemViewModel item)
        {
            item.HandleItemRemoved();
            OnItemRemoved(item);
        }

        protected virtual void OnItemRemoved(TItemViewModel item)
        {
        }

        protected abstract bool TryRemoveModelWithoutRecord(TItemModel model);

        #endregion
    }
}