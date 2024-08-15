#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkRuntimeConsts.AddAgentFacadeMenuPath)]
    public class UtilityAgentFacade : EntityFacade<UtilityAgent>
    {
        public void Register(UtilityWorld world)
        {
            Entity?.Register(world);
        }
    }
}