using UnityEngine;

namespace CarlosLab.Common
{
    public class GameObjectVariable : Variable<GameObject>
    {
        public static implicit operator GameObjectVariable(GameObject value)
        {
            return new GameObjectVariable { Value = value };
        }
    }
}
