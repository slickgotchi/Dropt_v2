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

    private SoundFX_SpiderPod m_soundFX_SpiderPod;

    private void Start()
    {
        GetComponent<Animator>().Play("SpiderPod_Idle");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_soundFX_SpiderPod = GetComponent<SoundFX_SpiderPod>();
        m_soundFX_SpiderPod.PlaySpawnSound();
    }

    private void Update()
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

    private void Burst()
    {
        GetComponent<Animator>().Play("SpiderPod_Burst");
        PlayBurstSoundClientRpc();
        float startAngle = Random.Range(0, 360.0f);
        float deltaAngle = 360 / NumberSpiders;
        for (int i = 0; i < NumberSpiders; i++)
        {
            var dir = PlayerAbility.GetDirectionFromAngle(startAngle + deltaAngle * i);
            var spider = Instantiate(SpiderPrefab);
            spider.transform.position = transform.position + new Vector3(0f, 1f, 0f);
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDuration = SpawnDuration;
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDirection = dir.normalized;
            spider.GetComponent<Dropt.EnemyAI_Spider>().SpawnDistance = SpawnDistance;
            spider.SetActive(false);

            // DO NOT SPAWN DIRECTLY AFTER INSTANTIATING, UNITY NEEDS A FRAME TO ALLOW THE NAVMESH TO GET PICKED UP BY NEW SPIDERS
            // USE THE DEFERRED SPAWNER INSTEAD
            DeferredSpawner.SpawnNextFrame(spider.GetComponent<NetworkObject>());
        }
    }

    [ClientRpc]
    private void PlayBurstSoundClientRpc()
    {
        m_soundFX_SpiderPod.PlayBurstSound();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        m_soundFX_SpiderPod.PlayDieSound();
    }
}
