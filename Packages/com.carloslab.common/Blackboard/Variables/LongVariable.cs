namespace CarlosLab.Common
{
    public class LongVariable : Variable<long>
    {
        public static implicit operator LongVariable(long value)
        {
            return new LongVariable { Value = value };
        }
    }
}