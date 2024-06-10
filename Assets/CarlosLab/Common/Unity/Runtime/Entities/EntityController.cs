#region

using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

#endregion

namespace CarlosLab.Common
{
    // [RequireComponent(typeof(EntityFacade))]
    public abstract class EntityController<TEntity> : MonoBehaviour
        where TEntity : class, IEntity
    {
        private TEntity entity;
        
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;


        public int Id => entity?.Id ?? -1;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                if (entity != null) entity.Name = name;
            }
        }
        
        public TEntity Entity
        {
            get
            {
                Init();
                return entity;
            }
        }

        protected void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            entity?.Enable();;
        }

        private void OnDisable()
        {
            entity?.Disable();
        }

        protected virtual void Init()
        {
            if (entity != null)
                return;

            entity = CreateEntity();
            entity.Name = name;
            IEntityFacade entityFacade = GetComponent<IEntityFacade>();
            entityFacade.Entity = entity;
            entity.EntityFacade = entityFacade;

            OnInit();
        }

        protected virtual void OnInit()
        {
        }
        
        protected abstract TEntity CreateEntity();


        public void Destroy()
        {
            if (entity != null)
            {
                entity.Destroy();
                entity = null;
            }
        }

        public void Unregister()
        {
            entity?.Unregister();
        }

        protected void Notify([CallerMemberName] string property = "")
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }

    public abstract class EntityController<TEntity, TDataAsset> : EntityController<TEntity>
        where TEntity : class, IEntity
        where TDataAsset : ScriptableObject, IDataAsset
    {
        [SerializeField]
        internal TDataAsset editorAsset;

        private TDataAsset runtimeAsset;

        [CreateProperty]
        public TDataAsset EditorAsset => editorAsset;

        public TDataAsset RuntimeAsset => runtimeAsset;

        public TDataAsset Asset
        {
            get
            {
                if (Application.isPlaying)
                    return runtimeAsset;
                return editorAsset;
            }
        }

        protected sealed override void Init()
        {
            InitRuntime();
            base.Init();
        }

        private void InitRuntime()
        {
            if (editorAsset == null)
            {
                CommonConsole.Instance.LogWarning($"No data asset assigned to {name}");
                return;
            }

            if (runtimeAsset != null)
                return;

            runtimeAsset = Instantiate(editorAsset);
            runtimeAsset.IsRuntimeAsset = true;
        }
    }
}