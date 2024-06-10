#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class IsGreaterThanOrEqualToValueNormalization<T> : InputNormalization<T>
    {
        public VariableReference<T> Value;
    }

    public class IsGreaterThanOrEqualToValueNormalizationFloat : IsGreaterThanOrEqualToValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            return rawInput >= Value ? 1.0f : 0.0f;
        }
    }

    public class IsGreaterThanOrEqualToValueNormalizationInt : IsGreaterThanOrEqualToValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            return rawInput >= Value ? 1.0f : 0.0f;
        }
    }
}