#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class Vector2IntVariable : Variable<Vector2Int>
    {
        public Vector2IntVariable()
        {
        }

        public Vector2IntVariable(Vector2Int value) : base(value)
        {
        }

        public static implicit operator Vector2IntVariable(Vector2Int value)
        {
            return new Vector2IntVariable { Value = value };
        }
    }
}