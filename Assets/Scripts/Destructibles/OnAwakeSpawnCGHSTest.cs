using UnityEngine;

namespace Destructibles
{
    //test
    public class OnAwakeSpawnCGHSTest : MonoBehaviour
    {
        private void Start()
        {
            PickupItemManager.Instance.SpawnSmallCGHST(Vector3.up * 7);
            PickupItemManager.Instance.SpawnBigCGHST(Vector3.up * 5);
        }
    }
}