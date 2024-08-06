using System.Collections.Generic;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TrapsGroup
    {
        public readonly List<Trap> Traps;
        public float CooldownTimer;

        public TrapsGroup()
        {
            Traps = new List<Trap>();
        }

        public void TryToUpdateCooldown(DamagedTrap damagedTrap)
        {
            //check if this trap is first in list
            for (int i = 0; i < Traps.Count; i++)
            {
                if (null == Traps[i])
                    continue;

                if (Traps[i] == damagedTrap)
                    break;

                return;
            }

            CooldownTimer -= Time.deltaTime;
        }

        public void ResetCooldown(float duration)
        {
            CooldownTimer = duration;
        }
    }

    public sealed class TrapsGroupSpawner : MonoBehaviour
    {
        public TrapSpawner[] Spawners;
    }
}