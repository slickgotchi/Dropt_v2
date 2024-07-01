using System.Collections.Generic;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Singleton { get; private set; }

    [SerializeField] private GameObject cloudExplosionPrefab;
    [SerializeField] private GameObject bulletExplosionPrefab;

    private Queue<GameObject> cloudExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> bulletExplosionPool = new Queue<GameObject>();

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

        if (cloudExplosionPool.Count > 0)
        {
            instance = cloudExplosionPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(cloudExplosionPrefab);
        }

        instance.transform.position = position;
        return instance;
    }

    public GameObject SpawnBulletExplosion(Vector3 position)
    {
        GameObject instance;

        if (bulletExplosionPool.Count > 0)
        {
            instance = bulletExplosionPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(bulletExplosionPrefab);
        }

        instance.transform.position = position;
        return instance;
    }

    public void ReturnToPool(GameObject instance)
    {
        instance.SetActive(false);

        if (instance.HasComponent<CloudExplosion>()) cloudExplosionPool.Enqueue(instance);
        if (instance.HasComponent<BulletExplosion>()) bulletExplosionPool.Enqueue(instance);
        
    }
}
