#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class Vector2IntVariable : Variable<Vector2Int>
    {
        public static implicit operator Vector2IntVariable(Vector2Int value)
        {
            return new Vector2IntVariable { Value = value };
        }
    }
}