using System.Collections.Generic;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TrapsGroup
    {
        public readonly List<Trap> Traps;

        public TrapsGroup()
        {
            Traps = new List<Trap>();
        }
    }

    public sealed class TrapsGroupSpawner : MonoBehaviour
    {
        public TrapSpawner[] Spawners;
    }
}