#region

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public class IsInRangeNormalizationFloat : InRangeNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            return rawInput >= MinValue && rawInput <= MaxValue ? 1.0f : 0.0f;
        }
    }

    public class IsInRangeNormalizationInt : InRangeNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            return rawInput >= MinValue && rawInput <= MaxValue ? 1.0f : 0.0f;
        }
    }
}