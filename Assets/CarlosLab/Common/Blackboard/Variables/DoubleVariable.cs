namespace CarlosLab.Common
{
    public class DoubleVariable : Variable<double>
    {
        public DoubleVariable()
        {
        }

        public DoubleVariable(double value) : base(value)
        {
        }

        public static implicit operator DoubleVariable(double value)
        {
            return new DoubleVariable { Value = value };
        }
    }
}