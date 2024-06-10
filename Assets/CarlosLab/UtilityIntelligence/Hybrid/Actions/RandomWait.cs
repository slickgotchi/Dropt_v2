#region

using System;
using CarlosLab.Common;
using CarlosLab.Common.Extensions;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class RandomWait : ActionTask
    {
        private readonly Random random = new();
        private float elapsedTime;

        private float waitTime;
        public VariableReference<float> WaitTimeMax;
        public VariableReference<float> WaitTimeMin;

        protected override void OnStart()
        {
            elapsedTime = 0;
            waitTime = random.NextFloat(WaitTimeMin, WaitTimeMax);
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime > waitTime) return UpdateStatus.Success;
            return UpdateStatus.Running;
        }
    }
}