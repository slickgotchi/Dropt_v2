namespace CarlosLab.UtilityIntelligence.Demos
{
    public class HealthInput : InputFromSource<int>
    {
        protected override int OnGetRawInput(InputContext context)
        {
            UtilityEntity inputSource = GetInputSource(context);
            if (inputSource.EntityFacade is Character character)
            {
                return character.Health.Health;
            }

            return 0;
        }
    }
}