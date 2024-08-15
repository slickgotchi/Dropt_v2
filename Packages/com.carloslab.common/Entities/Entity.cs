namespace CarlosLab.Common
{
    public abstract class Entity<TEntity, TWorld> : IEntity<TWorld>
        where TWorld : World<TWorld, TEntity>
        where TEntity : Entity<TEntity, TWorld>
    {
        #region EntityObject

        private IEntityFacade entityFacade;

        IEntityFacade IEntity.EntityFacade
        {
            get => entityFacade;
            set => entityFacade = value;
        }

        public IEntityFacade EntityFacade => entityFacade;

        #endregion

        #region Entity's Info

        private int id = -1;

        public int Id => id;

        private string name;
        
        string IEntity.Name
        {
            get => name;
            set => name = value;
        }

        public string Name => name;

        private EntityState state;

        public EntityState State => state;

        public bool IsActive => entityFacade is { IsActive: true };

        #endregion

        #region Entity/World's Info

        private TWorld world;
        public TWorld World => world;

        public bool Registerable => State == EntityState.None && IsActive;

        #endregion

        #region Entity's Methods

        public void Destroy()
        {
            if (State == EntityState.None)
            {
                OnDestroyed();
                return;
            }

            if (world == null)
            {
                StaticConsole.LogWarning($"Entity Name: {Name} Register World is null");
                return;
            }

            if (state != EntityState.Registered)
                return;

            state = EntityState.Destroying;

            world.Destroy(this as TEntity);
        }

        internal void OnDestroyed()
        {
            if (entityFacade != null)
            {
                entityFacade.DestroyInternal();
                entityFacade = null;
            }

            state = EntityState.Destroyed;
        }

        public T GetComponent<T>()
        {
            return entityFacade != null ? entityFacade.GetComponent<T>() : default;
        }

        public T GetComponentInChildren<T>()
        {
            return entityFacade != null ? entityFacade.GetComponentInChildren<T>() : default;
        }

        #endregion

        #region Register/Uregister World Methods

        public void Register(TWorld world)
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Entity Name: {Name} Register World is null");
                return;
            }

            if (!Registerable)
                return;

            state = EntityState.Registering;
            world.Register(this as TEntity);
        }

        internal void OnRegistered(int id, TWorld world)
        {
            this.id = id;
            this.world = world;
            state = EntityState.Registered;
        }

        public void Unregister()
        {
            if (world == null)
            {
                StaticConsole.LogWarning($"Entity Name: {Name} Register World is null");
                return;
            }

            if (State != EntityState.Registered)
                return;

            state = EntityState.Unregistering;

            world.Unregister(this as TEntity);
        }

        public void Enable()
        {
            world?.EnableEntity(this as TEntity);
        }

        public void Disable()
        {
            world?.DisableEntity(this as TEntity);
        }

        internal void OnUnregistered()
        {
            id = -1;
            state = EntityState.None;
        }

        #endregion
    }
}