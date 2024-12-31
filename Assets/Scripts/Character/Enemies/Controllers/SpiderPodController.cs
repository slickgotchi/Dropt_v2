using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

namespace Dropt
{
    public class SpiderPodController : NetworkBehaviour
    {
        public GameObject SpiderPrefab;
        public float BurstRange = 4f;
        public int NumberSpiders = 3;
        public float SpawnDuration = 1f;
        public float SpawnDistance = 2f;

        private bool m_isBurst = false;

        private SoundFX_SpiderPod m_soundFX_SpiderPod;

        private Animator m_animator;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_soundFX_SpiderPod = GetComponent<SoundFX_SpiderPod>();
            m_soundFX_SpiderPod.PlaySpawnSound();
            m_animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!IsServer) return;

            // check if time to burst pod
            if (!m_isBurst)
            {
                var players = Game.Instance.playerControllers;
                bool isBurstTime = false;
                foreach (PlayerController player in players)
                {
                    float dist = (player.transform.position - transform.position).magnitude;
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
            Utils.Anim.Play(m_animator, "SpiderPod_Burst");
            PlayBurstSoundClientRpc();
            float startAngle = Random.Range(0, 360.0f);
            float deltaAngle = 360 / NumberSpiders;
            for (int i = 0; i < NumberSpiders; i++)
            {
                Vector3 dir = PlayerAbility.GetDirectionFromAngle(startAngle + deltaAngle * i);
                //GameObject spider = Core.Pool.NetworkObjectPool.Instance.GetNetworkObject(SpiderPrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity).gameObject;
                GameObject spider = Instantiate(SpiderPrefab);
                spider.transform.position = transform.position + new Vector3(0f, 1f, 0f);
                EnemyAI_Spider enemyAI_Spider = spider.GetComponent<EnemyAI_Spider>();
                enemyAI_Spider.SpawnDuration = SpawnDuration;
                enemyAI_Spider.SpawnDirection = dir.normalized;
                enemyAI_Spider.SpawnDistance = SpawnDistance;
                spider.SetActive(false);

                // DO NOT SPAWN DIRECTLY AFTER INSTANTIATING, UNITY NEEDS A FRAME TO ALLOW THE NAVMESH TO GET PICKED UP BY NEW SPIDERS
                // USE THE DEFERRED SPAWNER INSTEAD
                DeferredSpawner.SpawnNextFrame(spider.GetComponent<NetworkObject>());
            }
            //await UniTask.Delay(1000);
            //Utils.Anim.Play(m_animator, "SpiderPod_Idle");
        }

        [ClientRpc]
        private void PlayBurstSoundClientRpc()
        {
            m_soundFX_SpiderPod.PlayBurstSound();
        }
    }
}
