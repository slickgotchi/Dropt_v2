namespace CarlosLab.Common
{
    public enum EntityState
    {
        None,
        Registering,
        Registered,
        Unregistering,
        Destroying,
        Destroyed
    }

    public interface IEntity
    {
        int Id { get; }

        string Name { get; internal set; }

        IEntityFacade EntityFacade { get; internal set; }

        bool IsActive { get; }

        EntityState State { get; }
        bool Registerable { get; }

        T GetComponent<T>();
        T GetComponentInChildren<T>();
        void Destroy();
        void Unregister();

        void Enable();
        void Disable();
    }

    public interface IEntity<TWorld> : IEntity where TWorld : class, IWorld
    {
        TWorld World { get; }

        void Register(TWorld world);
    }
}