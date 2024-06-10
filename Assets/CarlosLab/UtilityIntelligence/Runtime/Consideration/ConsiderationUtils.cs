
namespace CarlosLab.UtilityIntelligence
{
    public static class ConsiderationUtils
    {
        public static float CompensateScore(in float considerationScore, in float modificationFactor)
        {
            float makeUpValue = (1.0f - considerationScore) * modificationFactor;
            return considerationScore + makeUpValue * considerationScore;
        }

        public static float CalculateModificationFactor(in int considerationCount)
        {
            return 1.0f - 1.0f / considerationCount;
        }
    }
}
