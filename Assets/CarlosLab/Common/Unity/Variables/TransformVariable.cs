using UnityEngine;

namespace CarlosLab.Common
{
    public class TransformVariable : Variable<Transform>
    {
        public TransformVariable()
        {
        }

        public TransformVariable(Transform value) : base(value)
        {
        }

        public static implicit operator TransformVariable(Transform value)
        {
            return new TransformVariable { Value = value };
        }
    }
}