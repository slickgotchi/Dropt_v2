using System;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TrapsGroupSpawner : MonoBehaviour
    {
        public TrapSpawner[] Spawners;

        public int MaxGroupsCount { get; private set; }

        public void AddChild(int group)
        {
            MaxGroupsCount = Math.Max(group + 1, MaxGroupsCount);
        }
    }
}