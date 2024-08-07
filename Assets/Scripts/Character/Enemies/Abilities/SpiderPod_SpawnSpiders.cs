using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpiderPod_SpawnSpiders : EnemyAbility
{
    [Header("SpiderPod_SpawnSpiders Parameters")]
    public GameObject SpiderPrefab;
    public int NumberSpiders = 3;
    public float SpawnDuration = 1f;
    public float SpawnDistance = 2f;

    private float m_spawnTimer = 0f;
    private List<GameObject> m_spiders = new List<GameObject>();
    private List<Vector3> m_spiderDirections = new List<Vector3>();
    private float m_speed = 1f;

    public override void OnExecutionStart()
    {
        Parent.GetComponent<Animator>().Play("SpiderPod_Burst");

        float startAngle = UnityEngine.Random.Range(0, 360.0f);
        float deltaAngle = 360 / NumberSpiders;
        for (int i = 0; i < NumberSpiders; i++)
        {
            var dir = PlayerAbility.GetDirectionFromAngle(startAngle + deltaAngle * i);
            var spider = Instantiate(SpiderPrefab);
            spider.transform.position = Parent.transform.position + new Vector3(0f, 1f, 0f);
            spider.GetComponent<EnemyController>().SpawnDuration = SpawnDuration;
            spider.GetComponent<NetworkObject>().Spawn();
            m_spiderDirections.Add(dir);
            m_spiders.Add(spider);
        }

        m_speed = SpawnDistance / SpawnDuration;
    }

    public override void OnUpdate()
    {
        for (int i = 0; i < m_spiders.Count; i++)
        {
            m_spiders[i].transform.position += m_speed * m_spiderDirections[i] * Time.deltaTime;
        }
    }

    public override void OnFinish()
    {
        m_spiders.Clear();
        m_spiderDirections.Clear();
    }
}
