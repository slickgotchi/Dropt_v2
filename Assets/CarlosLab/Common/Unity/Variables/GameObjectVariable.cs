using UnityEngine;

namespace CarlosLab.Common
{
    public class GameObjectVariable : Variable<GameObject>
    {
        public GameObjectVariable()
        {
        }

        public GameObjectVariable(GameObject value) : base(value)
        {
        }

        public static implicit operator GameObjectVariable(GameObject value)
        {
            return new GameObjectVariable { Value = value };
        }
    }
}
