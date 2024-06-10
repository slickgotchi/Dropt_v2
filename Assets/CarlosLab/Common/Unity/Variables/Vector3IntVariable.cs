#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class Vector3IntVariable : Variable<Vector3Int>
    {
        public Vector3IntVariable()
        {
        }

        public Vector3IntVariable(Vector3Int value) : base(value)
        {
        }

        public static implicit operator Vector3IntVariable(Vector3Int value)
        {
            return new Vector3IntVariable { Value = value };
        }
    }
}