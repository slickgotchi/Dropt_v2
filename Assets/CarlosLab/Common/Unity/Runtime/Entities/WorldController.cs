#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public abstract class WorldController<TWorld> : MonoBehaviour
        where TWorld : IWorld
    {
        private TWorld world;

        public TWorld World
        {
            get
            {
                if (world == null)
                    world = CreateWorld();

                return world;
            }
        }

        private void Update()
        {
            World.Tick(Time.deltaTime);
        }

        private void OnEnable()
        {
            World.Start();
        }

        private void OnDisable()
        {
            World.Stop();
        }
        
        protected abstract TWorld CreateWorld();
    }
}