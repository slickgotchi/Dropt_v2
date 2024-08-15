namespace CarlosLab.Common
{
    public class FloatVariable : Variable<float>
    {
        public static implicit operator FloatVariable(float value)
        {
            return new FloatVariable { Value = value };
        }
    }
}