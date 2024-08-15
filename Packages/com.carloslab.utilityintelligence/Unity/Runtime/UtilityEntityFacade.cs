#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkRuntimeConsts.AddEntityFacadeMenuPath)]
    public class UtilityEntityFacade : EntityFacade<UtilityEntity>
    {
        public void Register(UtilityWorld world)
        {
            Entity?.Register(world);
        }
    }
}