using System;
using Chests.WeightRandom;
using UnityEngine;

namespace Chest
{
    [Serializable]
    public struct ChestConfig
    {
        public ActivePlayersData[] PlayersToPercent;
        public WeightVariable<Wearable.RarityEnum>[] Weapons;
        public float OrbsSpawnRange;
        public int ItemsDropCount;
    }

    [Serializable]
    public struct ActivePlayersData
    {
        public int[] Percents;
    }
}