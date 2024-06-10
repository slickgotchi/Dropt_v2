#region

using UnityEngine;

#endregion

namespace CarlosLab.Common
{
    public class Vector3Variable : Variable<Vector3>
    {
        public Vector3Variable()
        {
        }

        public Vector3Variable(Vector3 value) : base(value)
        {
        }

        public static implicit operator Vector3Variable(Vector3 value)
        {
            return new Vector3Variable { Value = value };
        }
    }
}