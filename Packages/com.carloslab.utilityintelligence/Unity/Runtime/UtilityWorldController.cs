#region

using CarlosLab.Common;
using UnityEngine;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    [AddComponentMenu(FrameworkRuntimeConsts.AddWorldControllerMenuPath)]
    public class UtilityWorldController : WorldController<UtilityWorld>
    {
        [SerializeField]
        private float makeDecisionInterval = 0.1f;

        protected override UtilityWorld CreateWorld()
        {
            return new UtilityWorld(makeDecisionInterval);
        }

        public UtilityEntity GetEntity(int id)
        {
            return World?.GetEntity(id);
        }
    }
}