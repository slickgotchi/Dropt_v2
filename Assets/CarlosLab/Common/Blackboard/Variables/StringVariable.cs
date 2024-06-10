namespace CarlosLab.Common
{
    public class StringVariable : Variable<string>
    {
        public StringVariable()
        {
        }

        public StringVariable(string value) : base(value)
        {
        }

        public static implicit operator StringVariable(string value)
        {
            return new StringVariable { Value = value };
        }
    }
}