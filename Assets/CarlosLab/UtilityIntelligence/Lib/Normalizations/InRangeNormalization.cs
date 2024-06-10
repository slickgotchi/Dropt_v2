#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class InRangeNormalization<TValue> : InputNormalization<TValue>
    {
        public VariableReference<TValue> MaxValue;
        public VariableReference<TValue> MinValue;
    }

    public class InRangeNormalizationFloat : InRangeNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            var diff = MaxValue - MinValue;
            if (diff <= 0.0f) return 0.0f;

            return (rawInput - MinValue) / (diff);
        }
    }

    public class InRangeNormalizationInt : InRangeNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            var diff = MaxValue - MinValue;
            if (diff <= 0) return 0.0f;

            return (float)(rawInput - MinValue) / (diff);
        }
    }
}