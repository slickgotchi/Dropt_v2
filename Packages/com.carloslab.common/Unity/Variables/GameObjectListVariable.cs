using System.Collections.Generic;
using UnityEngine;

namespace CarlosLab.Common
{
    public class GameObjectListVariable : Variable<List<GameObject>>
    {
        public static implicit operator GameObjectListVariable(List<GameObject> value)
        {
            return new GameObjectListVariable { Value = value };
        }
    }
}
