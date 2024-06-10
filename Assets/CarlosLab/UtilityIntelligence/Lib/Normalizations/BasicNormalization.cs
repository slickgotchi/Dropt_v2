namespace CarlosLab.UtilityIntelligence
{
    public abstract class BasicNormalization<T> : InputNormalization<T>
    {
    }

    public class BasicNormalizationBool : BasicNormalization<bool>
    {
        protected override float OnCalculateNormalizedInput(bool rawInput, InputContext context)
        {
            return rawInput ? 1.0f : 0.0f;
        }
    }

    public class BasicNormalizationFloat : BasicNormalization<float>
    {
        protected override float OnCalculateNormalizedInput(float rawInput, InputContext context)
        {
            return rawInput;
        }
    }

    public class BasicNormalizationInt : BasicNormalization<int>
    {
        protected override float OnCalculateNormalizedInput(int rawInput, InputContext context)
        {
            return rawInput;
        }
    }
}