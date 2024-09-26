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

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Animator>().Play("SpiderPod_Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        // check if time to burst pod
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

            if (isBurstTime)
            {
                Burst();
                m_isBurst = true;
            }
        }
    }

    void Burst()
    {
        GetComponent<Animator>().Play("SpiderPod_Burst");

        float startAngle = UnityEngine.Random.Range(0, 360.0f);
        float deltaAngle = 360 / NumberSpiders;
        for (int i = 0; i < NumberSpiders; i++)
        {
            var dir = PlayerAbility.GetDirectionFromAngle(startAngle + deltaAngle * i);
            var spider = Instantiate(SpiderPrefab);
            spider.transform.position = transform.position + new Vector3(0f, 1f, 0f);
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDuration = SpawnDuration;
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDirection = dir.normalized;
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDistance = SpawnDistance;
            spider.GetComponent<NetworkObject>().Spawn();
        }
    }
}
