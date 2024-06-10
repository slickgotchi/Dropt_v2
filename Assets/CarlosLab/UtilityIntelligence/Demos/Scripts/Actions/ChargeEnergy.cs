namespace CarlosLab.UtilityIntelligence.Demos
{
    public class ChargeEnergy : ActionTask
    {
        protected override UpdateStatus OnUpdate(float deltaTime)
        {
            if (Context.TargetFacade is ChargeStation { Type: ChargeStationType.EnergyStation } energyStation)
            {
                int chargeEnergy = energyStation.ChargePerSec;
                CharacterEnergy energy = GetComponent<CharacterEnergy>();
                energy.Energy += chargeEnergy;

                return UpdateStatus.Success;
            }

            return UpdateStatus.Failure;
        }
    }
}