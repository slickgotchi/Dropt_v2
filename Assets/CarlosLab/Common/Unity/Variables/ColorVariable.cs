#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class ColorVariable : Variable<Color>
    {
        public ColorVariable()
        {
        }

        public ColorVariable(Color value) : base(value)
        {
        }

        public static implicit operator ColorVariable(Color value)
        {
            return new ColorVariable { Value = value };
        }
    }
}