#region

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;

#endregion

namespace CarlosLab.Common.UI
{
    public abstract class BaseListViewModel<TContainerModel, TItemModel, TItemViewModel> :
        BaseListViewModel<TItemModel, TItemViewModel>, IViewModelWithModel<TContainerModel>
        where TContainerModel : class
        where TItemModel : class, IModelWithId
        where TItemViewModel : BaseItemViewModel<TItemModel>
    {
        private TContainerModel model;

        protected BaseListViewModel(IDataAsset asset, TContainerModel model) : base(asset, model)
        {
            if (model == null) return;
            
            this.model = model;
            OnRegisterModelEvents(model);
            InitItems();
        }

        protected virtual void InitItems()
        {
            items.Clear();

            for (int index = 0; index < ItemModels.Count; index++)
            {
                TItemModel itemModel = ItemModels[index];
                TryCreateItemWithoutModel(itemModel, out _);
            }
        }

        public TItemViewModel GetItemById(string id)
        {
            return items.Find(x => x.ModelId == id);
        }

        public TItemModel GetModelById(string id)
        {
            return ItemModels.FirstOrDefault(x => x.Id == id);
        }

        public override void ResetModel()
        {
            Model = null;
        }

        protected virtual void OnRegisterModelEvents(TContainerModel model)
        {
        }

        protected virtual void OnUnregisterModelEvents(TContainerModel model)
        {
        }

        private void HandleModelChanged(TContainerModel newModel)
        {
            TItemModel[] models = ItemModels.ToArray();
            if (models.Length > items.Count)
            {
                for (int index = 0; index < models.Length; index++)
                {
                    TItemModel model = models[index];
                    TItemViewModel item = GetItemById(model.Id);
                    if (item != null)
                        item.Model = model;
                    else
                        TryCreateItemWithoutModel(model, index, out _, true);
                }
            }
            else if (models.Length == items.Count)
            {
                for (int index = 0; index < models.Length; index++)
                {
                    TItemModel model = models[index];
                    TItemViewModel item = GetItemById(model.Id);
                    if (item != null)
                        item.Model = model;
                }
            }
            else
            {
                foreach (TItemViewModel item in items.ToArray())
                {
                    TItemModel model = GetModelById(item.Model.Id);
                    if (model != null)
                        item.Model = model;
                    else
                        TryRemoveItemWithoutModel(item, true);
                }
            }

            OnModelChanged(newModel);
            RaiseModelChanged();
        }

        protected virtual void OnModelChanged(TContainerModel newModel)
        {
        }

        #region Properties

        public TContainerModel Model
        {
            get => model;
            set
            {
                if (model == value) return;
                if (model != null) OnUnregisterModelEvents(model);

                model = value;

                if (model != null) OnRegisterModelEvents(model);
                HandleModelChanged(model);
            }
        }

        public override object ModelObject => model;

        #endregion
    }
}