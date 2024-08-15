using NUnit.Framework;
using Unity.PerformanceTesting;

namespace CarlosLab.Common.Benchmarks
{
    public class InfluenceCurveBenchmark
    {
        private InfluenceCurve responseCurve = InfluenceCurve.BasicLogistic;

        private float input = 0.9f;
        
        private int warmupCount = 3;
        private int measurementCount = 10;
        private int iterationsPerMeasurement = 100000;
        
        [Test, Performance]
        public void EvaluateInfluenceCurve()
        {
            Measure.Method(
                    () =>
                    {
                        InfluenceCurveUtils.Evaluate(input, in responseCurve);
                    })
                .WarmupCount(warmupCount)
                .MeasurementCount(measurementCount)
                .IterationsPerMeasurement(iterationsPerMeasurement)
                .Run();
        }
    }
}
