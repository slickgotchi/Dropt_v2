using UnityEngine;

namespace CarlosLab.Common
{
    public class TransformVariable : Variable<Transform>
    {
        public static implicit operator TransformVariable(Transform value)
        {
            return new TransformVariable { Value = value };
        }
    }
}