namespace CarlosLab.Common
{
    public class BoolVariable : Variable<bool>
    {
        public static implicit operator BoolVariable(bool value)
        {
            return new BoolVariable { Value = value };
        }
    }
}