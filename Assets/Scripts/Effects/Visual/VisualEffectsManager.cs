using System.Collections.Generic;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Singleton { get; private set; }

    [SerializeField]
    private GameObject cloudExplosionPrefab;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject SpawnCloudExplosion(Vector3 position)
    {
        GameObject instance;

        if (pool.Count > 0)
        {
            instance = pool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(cloudExplosionPrefab);
        }

        instance.transform.position = position;
        return instance;
    }

    public void ReturnToPool(GameObject instance)
    {
        instance.SetActive(false);
        pool.Enqueue(instance);
    }
}
