#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [RequireComponent(typeof(UtilityEntityFacade))]
    [AddComponentMenu(FrameworkRuntimeConsts.AddEntityControllerMenuPath)]
    public class UtilityEntityController : EntityController<UtilityEntity>
    {
        protected override UtilityEntity CreateEntity()
        {
            return new UtilityEntity();
        }

        public void Register(UtilityWorldController world)
        {
            Entity?.Register(world.World);
        }
    }
}