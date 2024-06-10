#region

using CarlosLab.Common;

#endregion

namespace CarlosLab.UtilityIntelligence
{
    public abstract class DivideByMaxValueNormalization<TValue> : InputNormalization<TValue>
    {
        public VariableReference<TValue> MaxValue;
    }
    
    public class DivideByMaxValueNormalizationFloat : DivideByMaxValueNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            if (MaxValue == 0.0f) return 0.0f;
            return rawInput / MaxValue;
        }
    }
    
    public class DivideByMaxValueNormalizationInt : DivideByMaxValueNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            if (MaxValue == 0) return 0;
            
            return (float)rawInput / MaxValue;
        }
    }
}