using UnityEngine;

public class CrystalDoorSpawner : MonoBehaviour
{
    [HideInInspector] public int spawnerId = -1;

    [SerializeField] private GameObject m_doorsPrefab;

    public GameObject GetRandomDoorPrefab()
    {
        return m_doorsPrefab;
    }

#if UNITY_EDITOR
    private void Awake()
    {
        if (m_doorsPrefab == null)
        {
            Debug.LogWarning("DOOR PREFAB IS NOT ASSIGN IN CRYSTAL DOOR SPAWNER ", this);
        }
    }
#endif

}