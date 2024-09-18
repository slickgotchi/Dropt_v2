using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpiderPodController : NetworkBehaviour
{
    public GameObject SpiderPrefab;
    public float BurstRange = 4f;
    public int NumberSpiders = 3;
    public float SpawnDuration = 1f;
    public float SpawnDistance = 2f;

    private bool m_isBurst = false;
    private List<GameObject> m_spiders = new List<GameObject>();
    private List<Vector3> m_spiderDirections = new List<Vector3>();
    private float m_speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        m_speed = SpawnDistance / SpawnDuration;

        GetComponent<Animator>().Play("SpiderPod_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        if (!m_isBurst)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            bool isBurstTime = false;
            foreach (var player in players)
            {
                var dist = (player.transform.position - transform.position).magnitude;
                if (dist < BurstRange)
                {
                    isBurstTime = true;
                }
             }

            if (isBurstTime) Burst();
        }

        for (int i = 0; i < m_spiders.Count; i++)
        {
            if (m_spiders[i] == null) continue;
            m_spiders[i].transform.position += m_speed * m_spiderDirections[i] * Time.deltaTime;
        }
    }

    void Burst()
    {
        GetComponent<Animator>().Play("SpiderPod_Burst");

        m_isBurst = true;

        float startAngle = UnityEngine.Random.Range(0, 360.0f);
        float deltaAngle = 360 / NumberSpiders;
        for (int i = 0; i < NumberSpiders; i++)
        {
            var dir = PlayerAbility.GetDirectionFromAngle(startAngle + deltaAngle * i);
            var spider = Instantiate(SpiderPrefab);
            spider.transform.position = transform.position + new Vector3(0f, 1f, 0f);
            spider.GetComponent<EnemyController>().SpawnDuration = SpawnDuration;
            spider.GetComponent<NetworkObject>().Spawn();
            m_spiderDirections.Add(dir);
            m_spiders.Add(spider);
        }
    }
}
