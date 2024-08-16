using System;
using UnityEngine;

namespace Chest
{
    [Serializable]
    public struct ChestConfig
    {
        public ActivePlayersData[] PlayersToPercent;
        public WeaponData[] Weapons;
        public int MinCGHST;
        public int MaxCGHST;
        public int MinGltr;
        public int MaxGltr;
        public float OrbsSpawnRange;
    }

    [Serializable]
    public struct ActivePlayersData
    {
        public int[] Percents;
    }

    [Serializable]
    public struct WeaponData
    {
        public Wearable.RarityEnum Rarity;
        [Range(0, 100)] public int Chance;
    }
}