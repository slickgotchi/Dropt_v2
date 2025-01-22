using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using Unity.Mathematics;

public class FussPot_EruptProjectile : NetworkBehaviour
{
    [Header("FussPot_EruptProjectile Parameters")]
    //public float HitRadius = 3f;
    public GameObject TargetMarker;
    public LobArc LobArcBody;

    [HideInInspector] public Vector3 Direction;
    [HideInInspector] public float Distance;
    [HideInInspector] public float Duration;

    [HideInInspector] public float DamagePerHit = 1f;
    [HideInInspector] public float CriticalChance = 0.1f;
    [HideInInspector] public float CriticalDamage = 1.5f;

    [HideInInspector] public float Scale = 1f;

    private float m_timer = 0;
    private float m_speed = 1;
    private bool m_isCollided = false;

    private Collider2D m_collider;
    [SerializeField] private Animator m_animator;

    private Vector3 m_finalPosition = Vector3.zero;

    private SoundFX_ProjectileHitGround m_soundFX_ProjectileHitGround;

    public List<SpriteRenderer> m_spriteRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        m_collider = GetComponentInChildren<Collider2D>();
        m_soundFX_ProjectileHitGround = GetComponent<SoundFX_ProjectileHitGround>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SetVisible(false);
    }

    void SetVisible(bool isVisible)
    {
        foreach (var sr in m_spriteRenderers)
        {
            sr.enabled = isVisible;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public void Init(Vector3 position, Quaternion rotation, Vector3 direction, float distance, float duration, float damagePerHit,
        float criticalChance, float criticalDamage)
    {
        transform.SetPositionAndRotation(position, rotation);
        Direction = direction.normalized;
        Distance = distance;
        Duration = duration;
        DamagePerHit = damagePerHit;
        CriticalChance = criticalChance;
        CriticalDamage = criticalDamage;

        m_finalPosition = transform.position + Direction * Distance;
        TargetMarker.transform.parent = null;
        TargetMarker.transform.position = m_finalPosition;

        //m_collider.GetComponent<CircleCollider2D>().radius = HitRadius;

    }

    public void Fire()
    {
        SetVisible(true);
        m_isCollided = false;
        m_timer = Duration;
        m_speed = Distance / Duration;

        LobArcBody.Duration_s = Duration;
        LobArcBody.Reset();

        if (IsClient)
        {
            Dropt.Utils.Anim.PlayAnimationWithDuration(m_animator, "Fusspot_EruptProjectile", Duration);
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;

        m_timer -= Time.deltaTime;

        if (m_timer > 0 && !m_isCollided)
        {
            transform.position += m_speed * Time.deltaTime * Direction;
        }

        else if (m_timer <= 0 && !m_isCollided)
        {
            m_isCollided = true;

            if (IsClient)
            {
                SetVisible(false);
                VisualEffectsManager.Instance.SpawnCloudExplosion(transform.position);
                m_soundFX_ProjectileHitGround.Play();
            }

            else if (IsServer)
            {
                CheckDamageToAllPlayers_SERVER();
            }
        }
    }

    void CheckDamageToAllPlayers_SERVER()
    {
        if (!IsServer) return;

        //m_collider.GetComponent<CircleCollider2D>().radius = HitRadius;

        var playerControllers = Game.Instance.playerControllers;

        // Create a list to track all the tasks
        //List<UniTask> damageTasks = new List<UniTask>();

        foreach (var playerController in playerControllers)
        {
            var isCritical = Dropt.Utils.Battle.IsCriticalAttack(CriticalChance);
            var damage = isCritical ? DamagePerHit * CriticalDamage : DamagePerHit;
            damage = Dropt.Utils.Battle.GetRandomVariation(damage);

            // Add each task to the list
            //damageTasks.Add(CheckDamageToPlayer_SERVER(playerController, damage, isCritical));
            CheckDamageToPlayer_SERVER(playerController, damage, isCritical);
        }
    }

    async UniTask CheckDamageToPlayer_SERVER(PlayerController playerController, float damage, bool isCritical)
    {
        var playerPing = playerController.GetComponent<PlayerPing>();
        if (playerPing != null)
        {
            // get the players hurt collider
            var playerHurtCollider = playerController.HurtCollider2D;

            var delay_s = NetworkTimer_v2.Instance.TickInterval * 2 +
                playerPing.RTT_ms.Value * 0.001f;
            delay_s = math.min(delay_s, 0.5f);

            Debug.Log("Player position before lagcomp: " + playerController.transform.position);

            await UniTask.Delay((int)(delay_s*1000));

            Debug.Log("Player position after lagcomp: " + playerController.transform.position);

            // sync colliders to current transform
            Physics2D.SyncTransforms();

            // do a collision check
            List<Collider2D> playerHurtColliders = new List<Collider2D>();

            m_collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHurtColliders);
            foreach (var hit in playerHurtColliders)
            {
                if (hit == playerHurtCollider)
                {
                    playerController.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, null);
                    Debug.Log("Player position after lagcomp (hit): " + playerController.transform.position);
                    break;
                }
            }

            // clear out colliders
            playerHurtColliders.Clear();
        }
    }

    /*
    async UniTaskVoid CheckDamageToPlayer_SERVER(float damage, bool isCritical)
    {
        if (!IsServer) return;

        // we need to delay the collision check to account for lag
        // - player interp is 2 ticks back of current position (check player interp in PlayerPrediction)
        // -
        var delay_s = NetworkTimer_v2.Instance.TickInterval * 2;
        if (IsHost) delay_s = 0;

        await UniTask.Delay((int)(1000 * delay_s));

        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> playerHitColliders = new List<Collider2D>();

        m_collider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
        foreach (var hit in playerHitColliders)
        {
            var player = hit.transform.parent;
            if (player.HasComponent<NetworkCharacter>())
            {
                player.GetComponent<NetworkCharacter>().TakeDamage(damage, isCritical, null);
            }
        }

        // clear out colliders
        playerHitColliders.Clear();

        gameObject.SetActive(false);
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
    */
}
