using CarlosLab.Common;

namespace CarlosLab.UtilityIntelligence
{
    public class IsInCooldownNormalization : InputNormalization<float>
    {
        public VariableReference<float> CooldownDuration;

        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            if (rawInput <= CooldownDuration)
                return 1.0f;
            else 
                return 0.0f;
        }
    }
}