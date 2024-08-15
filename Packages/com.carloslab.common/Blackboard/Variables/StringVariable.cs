namespace CarlosLab.Common
{
    public class StringVariable : Variable<string>
    {
        public static implicit operator StringVariable(string value)
        {
            return new StringVariable { Value = value };
        }
    }
}