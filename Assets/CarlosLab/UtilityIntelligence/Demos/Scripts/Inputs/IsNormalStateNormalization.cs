namespace CarlosLab.UtilityIntelligence.Demos
{
    public class IsNormalStateNormalization : InputNormalization<CharacterState>
    {
        protected override float OnCalculateNormalizedInput(CharacterState rawInput, InputContext context)
        {
            return rawInput == CharacterState.Normal ? 1.0f : 0.0f;
        }
    }
}