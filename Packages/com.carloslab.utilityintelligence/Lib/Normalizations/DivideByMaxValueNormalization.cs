#region

using System;
using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class DivideByMaxValueNormalization<TValue> : InputNormalization<TValue>
    {
        public VariableReference<TValue> MaxValue;
    }
    
    [Category("Division")]
    public class DivideByMaxValueNormalizationFloat : DivideByMaxValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, in InputNormalizationContext context)
        {
            if (MaxValue == 0.0f) return 0.0f;
            float normalizedInput = rawInput / MaxValue;
            return normalizedInput;
        }
    }
    
    [Category("Division")]
    public class DivideByMaxValueNormalizationInt : DivideByMaxValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, in InputNormalizationContext context)
        {
            if (MaxValue == 0) return 0;
            
            float normalizedInput = (float)rawInput / MaxValue;
            return normalizedInput;
        }
    }
}