namespace CarlosLab.Common
{
    public class DoubleVariable : Variable<double>
    {
        public static implicit operator DoubleVariable(double value)
        {
            return new DoubleVariable { Value = value };
        }
    }
}