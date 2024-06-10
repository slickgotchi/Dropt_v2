#region

using System.Collections.Generic;

#endregion

namespace CarlosLab.Common
{
    public interface IWorld
    {
        int UpdateTick { get; }

        bool IsRunning { get; }

        internal void Tick(float deltaTime);

        void Start();
        void Stop();
    }

    public interface IWorld<TEntity> : IWorld where TEntity : class, IEntity
    {
        TEntity GetEntity(int id);

        bool Contains(TEntity entity);

        internal void Register(TEntity entity);

        internal void Unregister(TEntity entity);

        internal void Destroy(TEntity entity);
    }
}