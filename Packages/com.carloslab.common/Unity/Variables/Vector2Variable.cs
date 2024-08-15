#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class Vector2Variable : Variable<Vector2>
    {
        public static implicit operator Vector2Variable(Vector2 value)
        {
            return new Vector2Variable { Value = value };
        }
    }
}