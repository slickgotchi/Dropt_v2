#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class IsLessThanOrEqualToValueNormalization<T> : InputNormalization<T>
    {
        public VariableReference<T> Value;
    }

    [Category("Comparison")]
    public class IsLessThanOrEqualToValueNormalizationFloat : IsLessThanOrEqualToValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            float normalizedInput =  rawInput <= Value ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }

    [Category("Comparison")]
    public class IsLessThanOrEqualToValueNormalizationInt : IsLessThanOrEqualToValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            float normalizedInput =  rawInput <= Value ? 1.0f : 0.0f;
            return normalizedInput;
        }
    }
}