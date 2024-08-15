using CarlosLab.Common;
using UnityEngine;

namespace CarlosLab.UtilityIntelligence
{
    public class StartCooldown : ActionTask
    {
        public VariableReference<float> CooldownStartTime;

        protected override void OnStart()
        {
            CooldownStartTime.Value = Time.time;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Success;
        }
    }
}