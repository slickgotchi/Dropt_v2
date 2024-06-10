namespace CarlosLab.Common
{
    public class BoolVariable : Variable<bool>
    {
        public BoolVariable()
        {
        }

        public BoolVariable(bool value) : base(value)
        {
        }

        public static implicit operator BoolVariable(bool value)
        {
            return new BoolVariable { Value = value };
        }
    }
}