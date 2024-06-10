namespace CarlosLab.Common
{
    public class IntVariable : Variable<int>
    {
        public IntVariable()
        {
        }

        public IntVariable(int value) : base(value)
        {
        }

        public static implicit operator IntVariable(int value)
        {
            return new IntVariable { Value = value };
        }
    }
}