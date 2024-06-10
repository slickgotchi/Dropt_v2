namespace CarlosLab.UtilityIntelligence
{
    public class Idle : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            return UpdateStatus.Running;
        }
    }
}