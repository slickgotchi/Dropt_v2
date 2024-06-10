namespace CarlosLab.Common
{
    public class LongVariable : Variable<long>
    {
        public LongVariable()
        {
        }

        public LongVariable(long value) : base(value)
        {
        }

        public static implicit operator LongVariable(long value)
        {
            return new LongVariable { Value = value };
        }
    }
}