using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    public class GameObjectListVariable : Variable<List<GameObject>>
    {
        public GameObjectListVariable()
        {
        }

        public GameObjectListVariable(List<GameObject> value) : base(value)
        {
        }

        public static implicit operator GameObjectListVariable(List<GameObject> value)
        {
            return new GameObjectListVariable { Value = value };
        }
    }
}
