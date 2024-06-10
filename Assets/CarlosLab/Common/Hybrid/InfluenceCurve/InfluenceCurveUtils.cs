#region

using System;
#if UNITY_BURST
using Unity.Burst;
#endif

#endregion

namespace CarlosLab.Common
{
#if UNITY_BURST
    [BurstCompile]
#endif
    public static class InfluenceCurveUtils
    {
        private static readonly float logisticPrecision = 0.01f;

#if UNITY_BURST
        [BurstCompile]
#endif
        public static float Evaluate(in float input, in InfluenceCurve data)
        {
            float score = 0.0f;
            switch (data.Type)
            {
                case InfluenceCurveType.Linear:
                    score = Sanitize(data.Slope * (input - data.XShift) + data.YShift);
                    break;
                case InfluenceCurveType.Polynomial:
                    score = Sanitize(data.Slope * MathUtils.Pow(input - data.XShift, data.Exponent) + data.YShift);
                    break;
                case InfluenceCurveType.Logistic:
                    score = Sanitize(
                        data.Slope / (1.0f + MathUtils.Exp(-10.0f * data.Exponent * (input - 0.5f - data.XShift))) +
                        data.YShift);
                    
                    if (score < logisticPrecision)
                        score = 0f;
                    else if (score > 1f - logisticPrecision)
                        score = 1f;
                    break;
                case InfluenceCurveType.Logit:
                    score = Sanitize(
                        data.Slope * MathUtils.Log((input - data.XShift) / (1.0f - (input - data.XShift))) / 5.0f +
                        0.5f + data.YShift);
                    break;
                case InfluenceCurveType.Normal:
                    score = Sanitize(data.Slope *
                                     MathUtils.Exp(-30.0f * data.Exponent * (input - data.XShift - 0.5f) *
                                                   (input - data.XShift - 0.5f)) +
                                     data.YShift);
                    break;
                case InfluenceCurveType.Sine:
                    score = Sanitize(0.5f * data.Slope * MathUtils.Sin(2.0f * MathF.PI * (input - data.XShift)) + 0.5f +
                                     data.YShift);
                    break;
            }

            return score;
        }

        private static float Sanitize(float y)
        {
            if (float.IsInfinity(y))
                return 0.0f;

            if (float.IsNaN(y))
                return 0.0f;

            if (y < 0.0f)
                return 0.0f;

            if (y > 1.0f)
                return 1.0f;

            return y;
        }
    }
}