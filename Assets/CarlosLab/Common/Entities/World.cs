#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace CarlosLab.Common
{
    public abstract class World<TWorld, TEntity> : IWorld<TEntity>
        where TWorld : World<TWorld, TEntity>
        where TEntity : Entity<TEntity, TWorld>
    {
        public World()
        {
            worldInternal = this;
        }

        #region Fields

        private readonly HashSet<int> freeEntityIds = new();
        private readonly Queue<int> freeEntityIdQueue = new();

        private readonly HashSet<TEntity> destroyingEntities = new();
        private readonly HashSet<TEntity> registeringEntities = new();
        private readonly HashSet<TEntity> unregisteringEntities = new();

        private readonly IWorld<TEntity> worldInternal;

        #endregion

        #region Poperties

        public int UpdateTick { get; private set; }

        public bool IsRunning { get; private set; }

        protected List<TEntity> EntityStore { get; } = new();

        public HashSet<TEntity> ActiveEntities { get; } = new();

        #endregion

        #region Lifecycles

        void IWorld.Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            Update(deltaTime);
            LateUpdate(deltaTime);
        }

        private void Update(float deltaTime)
        {
            UpdateTick++;
            OnUpdate(deltaTime);
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }

        private void LateUpdate(float deltaTime)
        {
            RegisterEntities();
            UnregisterEntities();
            DestroyEntities();

            OnLateUpdate(deltaTime);
        }

        protected virtual void OnLateUpdate(float deltaTime)
        {
        }

        public void Start()
        {
            IsRunning = true;
            OnStart();
        }

        protected virtual void OnStart()
        {
        }

        public void Stop()
        {
            IsRunning = false;
            OnStop();
        }

        protected virtual void OnStop()
        {
        }

        #endregion

        #region Entities

        public TEntity GetEntity(int id)
        {
            if (IdInBounds(id))
                return EntityStore[id];
            return null;
        }

        public bool Contains(TEntity entity)
        {
            return entity != null && IdInBounds(entity.Id) && EntityStore[entity.Id] == entity;
        }

        protected bool IdInBounds(int id)
        {
            return id >= 0 && id < EntityStore.Count;
        }

        void IWorld<TEntity>.Register(TEntity entity)
        {
            if (Contains(entity))
                return;

            if (!IsRunning)
            {
                RegisterInternal(entity);
            }
            else
            {
                registeringEntities.Add(entity);
            }
        }

        internal void Register(TEntity entity)
        {
            worldInternal.Register(entity);
        }

        private void RegisterInternal(TEntity entity)
        {
            if (TryGetFreeEntityId(out int entityId))
                EntityStore[entityId] = entity;
            else
            {
                EntityStore.Add(entity);
                entityId = EntityStore.Count - 1;
            }

            HandleEntityRegistered(entity, entityId);
        }

        private void HandleEntityRegistered(TEntity entity, int entityId)
        {
            entity.OnRegistered(entityId, this as TWorld);
            OnEntityRegistered(entity);
            
            if (entity.IsActive)
                EnableEntity(entity);
        }

        private void RegisterEntities()
        {
            foreach (TEntity entity in registeringEntities)
            {
                RegisterInternal(entity);
            }

            registeringEntities.Clear();
        }

        void IWorld<TEntity>.Unregister(TEntity entity)
        {
            if (!Contains(entity))
                return;

            unregisteringEntities.Add(entity);
        }

        internal void Unregister(TEntity entity)
        {
            worldInternal.Unregister(entity);
        }

        private void UnregisterInternal(TEntity entity)
        {
            if (!Contains(entity))
                return;

            HandleEntityUnregistering(entity);
            
            int entityId = entity.Id;
            EntityStore[entityId] = null;

            AddFreeEntityId(entityId);

            HandleEntityUnregistered(entity);
        }

        private void HandleEntityUnregistering(TEntity entity)
        {
            DisableEntity(entity);
        }

        private void HandleEntityUnregistered(TEntity entity)
        {
            entity.OnUnregistered();
            
            OnEntityUnregistered(entity);
        }

        private void UnregisterEntities()
        {
            foreach (TEntity entity in unregisteringEntities)
            {
                UnregisterInternal(entity);
            }

            unregisteringEntities.Clear();
        }

        private bool TryGetFreeEntityId(out int entityId)
        {
            if (freeEntityIdQueue.Count > 0)
            {
                entityId = freeEntityIdQueue.Dequeue();
                freeEntityIds.Remove(entityId);
                return true;
            }

            entityId = -1;
            return false;
        }

        private void AddFreeEntityId(int entityId)
        {
            if (freeEntityIds.Add(entityId))
                freeEntityIdQueue.Enqueue(entityId);
        }
        
        void IWorld<TEntity>.Destroy(TEntity entity)
        {
            if (!Contains(entity))
                return;

            destroyingEntities.Add(entity);
        }

        public void EnableEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;
            
            if (ActiveEntities.Add(entity))
                OnEntityEnabled(entity);
        }

        public void DisableEntity(TEntity entity)
        {
            if (!Contains(entity))
                return;
            
            if(ActiveEntities.Remove(entity))
                OnEntityDisabled(entity);
        }
        
        internal void Destroy(TEntity entity)
        {
            worldInternal.Destroy(entity);
        }

        private void DestroyInternal(TEntity entity)
        {
            UnregisterInternal(entity);
            HandleEntityDestroyed(entity);
        }

        private void HandleEntityDestroyed(TEntity entity)
        {
            entity.OnDestroyed();
            OnEntityDestroyed(entity);
        }

        private void DestroyEntities()
        {
            foreach (TEntity entity in destroyingEntities)
            {
                DestroyInternal(entity);
            }

            destroyingEntities.Clear();
        }

        #endregion

        #region Event Functions

        protected virtual void OnEntityRegistered(TEntity entity)
        {
        }

        protected virtual void OnEntityUnregistered(TEntity entity)
        {
        }
        
        protected virtual void OnEntityEnabled(TEntity entity)
        {
        }

        protected virtual void OnEntityDisabled(TEntity entity)
        {
        }

        protected virtual void OnEntityDestroyed(TEntity entity)
        {
        }

        #endregion
    }
}