namespace CarlosLab.UtilityIntelligence.Demos
{
    public class ChargeHealth : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (Context.TargetFacade is ChargeStation { Type: ChargeStationType.HealthStation } healthStation)
            {
                int chargeHealth = healthStation.ChargePerSec;
                CharacterHealth health = GetComponent<CharacterHealth>();
                health.Health += chargeHealth;

                return UpdateStatus.Success;
            }

            return UpdateStatus.Failure;
        }
    }
}