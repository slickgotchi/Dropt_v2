using UnityEngine;

namespace CarlosLab.Common
{
    public class AnimatorVariable : Variable<Animator>
    {
        public static implicit operator AnimatorVariable(Animator value)
        {
            return new AnimatorVariable { Value = value };
        }
    }
}