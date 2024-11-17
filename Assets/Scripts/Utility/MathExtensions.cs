using System.Collections.Generic;
using Unity.Mathematics;

public static class MathExtensions
{
    public static float CalculateMedian(List<float> values)
    {
        if (values == null || values.Count == 0)
            return 0;

        // Create a copy of the list to avoid modifying the original
        List<float> sortedValues = new List<float>(values);
        sortedValues.Sort();

        int count = sortedValues.Count;
        if (count % 2 == 0)
        {
            // If even, average the two middle elements
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2f;
        }
        else
        {
            // If odd, return the middle element
            return sortedValues[count / 2];
        }
    }

    public static float CalculateMean(List<float> values)
    {
        if (values == null || values.Count == 0)
            return 0;

        float sum = 0;
        foreach (float value in values)
        {
            sum += value;
        }

        return sum / values.Count;
    }
}
