namespace CarlosLab.Common
{
    public class FloatVariable : Variable<float>
    {
        public FloatVariable()
        {
        }

        public FloatVariable(float value) : base(value)
        {
        }

        public static implicit operator FloatVariable(float value)
        {
            return new FloatVariable { Value = value };
        }
    }
}