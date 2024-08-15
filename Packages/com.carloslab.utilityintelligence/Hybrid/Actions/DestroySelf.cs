

namespace CarlosLab.UtilityIntelligence
{
    public class DestroySelf : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            Agent.Destroy();
            return UpdateStatus.Success;
        }
    }
}