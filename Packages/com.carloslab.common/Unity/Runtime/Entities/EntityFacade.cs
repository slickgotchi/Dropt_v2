#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public abstract class EntityFacade<TEntity> : MonoBehaviour, IEntityFacade
        where TEntity : class, IEntity
    {
        #region Entity

        private TEntity entity;

        IEntity IEntityFacade.Entity
        {
            get => entity;
            set => entity = value as TEntity;
        }

        public TEntity Entity
        {
            get => entity;
            internal set => entity = value;
        }

        #endregion

        #region Entity's Info

        public string Name => name;

        public bool IsActive => gameObject.activeInHierarchy;

        public Float3 Position => transform.position;

        #endregion

        #region Entity's Methods

        public void Unregister()
        {
            entity?.Unregister();
        }
        
        public void Enable()
        {
            entity?.Enable();
        }

        public void Disable()
        {
            entity?.Disable();
        }

        public void Destroy()
        {
            entity?.Destroy();
        }

        void IEntityFacade.DestroyInternal()
        {
            Destroy(gameObject);
        }

        #endregion
    }
}