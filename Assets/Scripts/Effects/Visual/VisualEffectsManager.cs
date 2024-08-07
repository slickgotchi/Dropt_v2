using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Singleton { get; private set; }

    [SerializeField] private GameObject cloudExplosionPrefab;
    [SerializeField] private GameObject bulletExplosionPrefab;
    [SerializeField] private GameObject splashExplosionPrefab;
    [SerializeField] private GameObject fudWispExplosionPrefab;
    [SerializeField] private GameObject stompCirclePrefab;

    private Queue<GameObject> m_cloudExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_bulletExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_splashExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_fudWispExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_stompCirclePool = new Queue<GameObject>();

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

        if (m_cloudExplosionPool.Count > 0)
        {
            instance = m_cloudExplosionPool.Dequeue();
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

        if (m_bulletExplosionPool.Count > 0)
        {
            instance = m_bulletExplosionPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(bulletExplosionPrefab);
        }

        instance.transform.position = position;
        return instance;
    }

    public GameObject SpawnSplashExplosion(Vector3 position, Color? color = null, float scale = 1f)
    {
        if (color == null) color = Color.white;

        GameObject instance;

        if (m_splashExplosionPool.Count > 0)
        {
            instance = m_splashExplosionPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(splashExplosionPrefab);
        }

        instance.transform.position = position;
        instance.transform.localScale = new Vector3(scale, scale, 1);
        instance.GetComponentInChildren<SpriteRenderer>().color = (Color)color;
        return instance;
    }

    public GameObject SpawnFudWispExplosion(Vector3 position, Color? color = null, float scale = 1f)
    {
        if (color == null) color = Color.white;

        GameObject instance;

        if (m_fudWispExplosionPool.Count > 0)
        {
            instance = m_fudWispExplosionPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(fudWispExplosionPrefab);
        }

        instance.transform.position = position;
        instance.transform.localScale = new Vector3(scale, scale, 1);
        instance.GetComponentInChildren<SpriteRenderer>().color = (Color)color;
        return instance;
    }

    public GameObject SpawnStompCircle(Vector3 position)
    {
        GameObject instance;

        if (m_stompCirclePool.Count > 0)
        {
            instance = m_stompCirclePool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(stompCirclePrefab);
        }

        instance.transform.position = position;
        return instance;
    }

    public void ReturnToPool(GameObject instance)
    {
        instance.SetActive(false);

        if (instance.HasComponent<CloudExplosion>()) m_cloudExplosionPool.Enqueue(instance);
        if (instance.HasComponent<BulletExplosion>()) m_bulletExplosionPool.Enqueue(instance);
        if (instance.HasComponent<SplashExplosion>()) m_splashExplosionPool.Enqueue(instance);
        if (instance.HasComponent<FudWispExplosion>()) m_fudWispExplosionPool.Enqueue(instance);
        if (instance.HasComponent<StompCircle>()) m_stompCirclePool.Enqueue(instance);
    }
}
