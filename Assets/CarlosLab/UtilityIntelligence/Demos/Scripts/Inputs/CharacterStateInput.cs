namespace CarlosLab.UtilityIntelligence.Demos
{
    public class CharacterStateInput : InputFromSource<CharacterState>
    {
        protected override CharacterState OnGetRawInput(InputContext context)
        {
            UtilityEntity inputSource = GetInputSource(context);
            if (inputSource.EntityFacade is Character character)
            {
                return character.State;
            }

            return CharacterState.Normal;
        }
    }
}