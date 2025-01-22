using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance { get; private set; }

    [SerializeField] private GameObject cloudExplosionPrefab;
    [SerializeField] private GameObject bulletExplosionPrefab;
    [SerializeField] private GameObject splashExplosionPrefab;
    [SerializeField] private GameObject fudWispExplosionPrefab;
    [SerializeField] private GameObject stompCirclePrefab;
    [SerializeField] private GameObject basicCirclePrefab;
    [SerializeField] private GameObject petAttackPerfab;

    [SerializeField] private GameObject vfxDeathBloodSkullPrefab;
    //[SerializeField] private GameObject vfxBloodHit03Prefab;

    private Queue<GameObject> m_cloudExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_bulletExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_splashExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_fudWispExplosionPool = new Queue<GameObject>();
    private Queue<GameObject> m_stompCirclePool = new Queue<GameObject>();
    private Queue<GameObject> m_petAttackPool = new Queue<GameObject>();
    private Queue<GameObject> m_basicCirclePool = new Queue<GameObject>();

    private Queue<GameObject> m_vfxDeathBloodSkullPool = new Queue<GameObject>();
    //private Queue<GameObject> m_vfxBloodHit03Pool = new Queue<GameObject>();

    [SerializeField] private GameObject m_vfxDeathPrefab;

    [SerializeField] private List<GameObject> m_vfxBloodHitPrefabs = new List<GameObject>();
    private int m_vfxBloodHitCurrentIndex = 0;

    [SerializeField] private List<GameObject> m_vfxAttackHitPrefabs = new List<GameObject>();
    private int m_vfxAttackHitCurrentIndex = 0;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject SpawnCloudExplosion(Vector3 position)
    {
        GameObject instance;

        if (m_cloudExplosionPool.Count > 0)
        {
            instance = m_cloudExplosionPool.Dequeue();
            if (instance != null) instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(cloudExplosionPrefab);
        }

        if (instance != null) instance.transform.position = position;
        return instance;
    }

    //public GameObject SpawnBulletExplosion(Vector3 position)
    //{
    //    GameObject instance;

    //    if (m_bulletExplosionPool.Count > 0)
    //    {
    //        instance = m_bulletExplosionPool.Dequeue();
    //        instance?.SetActive(true);
    //    }
    //    else
    //    {
    //        instance = Instantiate(bulletExplosionPrefab);
    //    }

    //    if (instance != null) instance.transform.position = position;
    //    return instance;
    //}

    public GameObject SpawnSplashExplosion(Vector3 position, Color? color = null, float scale = 1f)
    {
        if (color == null) color = Color.white;

        GameObject instance;

        if (m_splashExplosionPool.Count > 0)
        {
            instance = m_splashExplosionPool.Dequeue();
            instance?.SetActive(true);
        }
        else
        {
            instance = Instantiate(splashExplosionPrefab);
        }

        if (instance != null)
        {
            instance.transform.position = position;
            instance.transform.localScale = new Vector3(scale, scale, 1);
            instance.GetComponentInChildren<SpriteRenderer>().color = (Color)color;
        }
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

    public GameObject SpawnBasicCircle(Vector3 position, Color? color = null, float radius = 0.5f)
    {
        if (color == null) color = Color.white;

        float scale = 2 * radius;

        GameObject instance;

        if (m_basicCirclePool.Count > 0)
        {
            instance = m_basicCirclePool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(basicCirclePrefab);
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

    public GameObject SpawnPetAttackEffect(Vector3 position)
    {
        GameObject instance;

        if (m_petAttackPool.Count > 0)
        {
            instance = m_petAttackPool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(petAttackPerfab);
        }

        instance.transform.position = position;
        return instance;
    }

    //public GameObject SpawnVFX_DeathBloodSkull(Vector3 position, float scale = 1f)
    //{
    //    GameObject instance = Instantiate(vfxDeathBloodSkullPrefab);
    //    instance.transform.position = position;
    //    instance.transform.localScale = new Vector3(scale, scale, 1);
    //    return instance;
    //}

    //public GameObject SpawnVFX_BloodHit_03(Vector3 position, float scale = 1f)
    //{
    //    m_vfxBloodHitCurrentIndex++;
    //    if (m_vfxBloodHitCurrentIndex >= m_vfxBloodHitPrefabs.Count)
    //    {
    //        m_vfxBloodHitCurrentIndex = 0;
    //    }

    //    GameObject instance = Instantiate(m_vfxBloodHitPrefabs[m_vfxBloodHitCurrentIndex]);
    //    instance.transform.position = position;
    //    instance.transform.localScale = new Vector3(scale, scale, 1);
    //    return instance;
    //}

    public GameObject Spawn_VFX_AttackHit(Vector3 position, float scale = 1f)
    {
        m_vfxAttackHitCurrentIndex++;
        if (m_vfxAttackHitCurrentIndex >= m_vfxAttackHitPrefabs.Count)
        {
            m_vfxAttackHitCurrentIndex = 0;
        }

        GameObject instance = Instantiate(m_vfxAttackHitPrefabs[m_vfxAttackHitCurrentIndex]);
        instance.transform.position = position;
        instance.transform.localScale = new Vector3(scale, scale, 1);
        return instance;
    }

    public GameObject Spawn_VFX_Death(Vector3 position, float scale = 1f)
    {
        GameObject instance = Instantiate(m_vfxDeathPrefab);
        instance.transform.position = position;
        instance.transform.localScale = new Vector3(scale, scale, 1);
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
        if (instance.HasComponent<BasicCircle>()) m_basicCirclePool.Enqueue(instance);
        if (instance.HasComponent<PetAttackEffect>()) m_petAttackPool.Enqueue(instance);

        if (instance.HasComponent<VFX_DeathBloodSkull>()) m_vfxDeathBloodSkullPool.Enqueue(instance);
        //if (instance.HasComponent<>)
    }
}
