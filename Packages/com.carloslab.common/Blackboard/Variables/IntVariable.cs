namespace CarlosLab.Common
{
    public class IntVariable : Variable<int>
    {
        public static implicit operator IntVariable(int value)
        {
            return new IntVariable { Value = value };
        }
    }
}