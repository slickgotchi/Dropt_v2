#region

using System;
#if UNITY_BURST
using Unity.Collections;
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
        public static float[] Evaluate(float[] inputs, InfluenceCurve[] curves)
        {
            NativeArray<float> inputArray = new(inputs, Allocator.Temp);
            NativeArray<InfluenceCurve> curveArray = new(curves, Allocator.Temp);
        
            Evaluate(in inputArray, in curveArray, out NativeArray<float> scoreArray);
            return scoreArray.ToArray();
        }
        
        [BurstCompile]
        private static void Evaluate(in NativeArray<float> inputs, in NativeArray<InfluenceCurve> curves, out NativeArray<float> scores)
        {
            scores = new(inputs.Length, Allocator.Temp);

            for (int index = 0; index < inputs.Length; index++)
            {
                float input = inputs[index];
                InfluenceCurve curve = curves[index];
                scores[index] = Evaluate(input, in curve);
            }
        }
#endif

#if UNITY_BURST
        [BurstCompile]
#endif
        public static float Evaluate(float input, in InfluenceCurve curve)
        {
            float score = 0.0f;
            switch (curve.Type)
            {
                case InfluenceCurveType.Linear:
                    score = Sanitize(curve.Slope * (input - curve.XShift) + curve.YShift);
                    break;
                case InfluenceCurveType.Polynomial:
                    score = Sanitize(curve.Slope * MathUtils.Pow(input - curve.XShift, curve.Exponent) + curve.YShift);
                    break;
                case InfluenceCurveType.Logistic:
                    score = Sanitize(
                        curve.Slope / (1.0f + MathUtils.Exp(-10.0f * curve.Exponent * (input - 0.5f - curve.XShift))) +
                        curve.YShift);
                    
                    if (score < logisticPrecision)
                        score = 0f;
                    else if (score > 1f - logisticPrecision)
                        score = 1f;
                    break;
                case InfluenceCurveType.Logit:
                    score = Sanitize(
                        curve.Slope * MathUtils.Log((input - curve.XShift) / (1.0f - (input - curve.XShift))) / 5.0f +
                        0.5f + curve.YShift);
                    break;
                case InfluenceCurveType.Normal:
                    score = Sanitize(curve.Slope *
                                     MathUtils.Exp(-30.0f * curve.Exponent * (input - curve.XShift - 0.5f) *
                                                   (input - curve.XShift - 0.5f)) +
                                     curve.YShift);
                    break;
                case InfluenceCurveType.Sine:
                    score = Sanitize(0.5f * curve.Slope * MathUtils.Sin(2.0f * MathF.PI * (input - curve.XShift)) + 0.5f +
                                     curve.YShift);
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