#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class IsLessThanOrEqualToValueNormalization<T> : InputNormalization<T>
    {
        public VariableReference<T> Value;
    }

    public class IsLessThanOrEqualToValueNormalizationFloat : IsLessThanOrEqualToValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            return rawInput <= Value ? 1.0f : 0.0f;
        }
    }

    public class IsLessThanOrEqualToValueNormalizationInt : IsLessThanOrEqualToValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            return rawInput <= Value ? 1.0f : 0.0f;
        }
    }
}