namespace CarlosLab.UtilityIntelligence.Demos
{
    public class EnergyInput : InputFromSource<int>
    {
        protected override int OnGetRawInput(InputContext context)
        {
            UtilityEntity inputSource = GetInputSource(context);
            if (inputSource.EntityFacade is Character character)
            {
                return character.Energy.Energy;
            }

            return 0;
        }
    }
}