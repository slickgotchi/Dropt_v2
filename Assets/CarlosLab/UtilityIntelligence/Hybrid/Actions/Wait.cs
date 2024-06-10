#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class Wait : ActionTask
    {
        private float elapsedTime;
        public VariableReference<float> WaitTime = 1.0f;

        protected override void OnStart()
        {
            elapsedTime = 0;
        }

        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (elapsedTime > WaitTime) return UpdateStatus.Success;
            return UpdateStatus.Running;
        }
    }
}