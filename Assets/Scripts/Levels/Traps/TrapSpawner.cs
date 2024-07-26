using UnityEngine;

namespace Level.Traps
{
    public sealed class TrapSpawner : MonoBehaviour
    {
        [SerializeField] public int Group;
        [SerializeField] public GameObject Prefab;
    }
}