﻿#region

using System.Runtime.Serialization;

#endregion

namespace CarlosLab.Common
{
    [DataContract]
    public abstract class ContainerItemModel<TContainer, TRuntime> : ModelWithId<TRuntime>, IContainerItem
        where TContainer : class, IItemContainer
        where TRuntime : class, IRuntimeObject, IContainerItem
    {
        #region IContainerItem

        [DataMember(Name = nameof(Name))]
        private string name;

        public string Name
        {
            get => name;
            internal set => name = value;
        }

        public bool IsInContainer => container != null;

        private TContainer container;
        public TContainer Container => container;
        
        void IContainerItem.HandleItemAdded(IItemContainer container, string name)
        {
            this.container = container as TContainer;
            this.name = name;
            OnItemAdded();
        }

        void IContainerItem.HandleItemRemoved()
        {
            this.container = null;
            OnItemRemoved();
        }

        string IContainerItem.Name
        {
            get => name;
            set => name = value;
        }
        
        protected virtual void OnItemAdded()
        {
            
        }

        protected virtual void OnItemRemoved()
        {
            
        }

        #endregion
    }
}