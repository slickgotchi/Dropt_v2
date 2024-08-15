using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    public class TransformListVariable : Variable<List<Transform>>
    {
        public static implicit operator TransformListVariable(List<Transform> value)
        {
            return new TransformListVariable { Value = value };
        }
    }
}
