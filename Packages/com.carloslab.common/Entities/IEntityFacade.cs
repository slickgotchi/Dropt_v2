namespace CarlosLab.Common
{
    public interface IEntityFacade
    {
        string Name { get; }
        IEntity Entity { get; internal set; }
        bool IsActive { get; }
        Float3 Position { get; }
        T GetComponent<T>();
        T GetComponentInChildren<T>();
        void Destroy();
        internal void DestroyInternal();
    }
}