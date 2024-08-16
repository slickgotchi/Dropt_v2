using Unity.Netcode;
using UnityEngine;

namespace Chest
{
    public class ChestsGroupSpawner : NetworkBehaviour
    {
        [SerializeField] private ChestSpawner[] m_spawners;
        public ChestSpawner[] Spawners => m_spawners;
    }
}