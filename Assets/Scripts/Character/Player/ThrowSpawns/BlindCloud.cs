using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BlindCloud : NetworkBehaviour
{
    private float m_timer = 1f;
    private ParticleSystem m_particleSystem;

    [SerializeField] private GameObject m_buffTimerPrefab;
    [SerializeField] private BuffObject m_buffObject;

    public float Radius = 1f;
    public float BlindDuration = 3f;

    private float m_scale = 1f;
    private Collider2D m_collider;

    private void Awake()
    {
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        m_collider = GetComponentInChildren<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (m_particleSystem != null)
        {
            // Get the main module and modify the simulation speed
            var mainModule = m_particleSystem.main;
            mainModule.simulationSpeed = 3f;

            // Adjust the particle system for scaling
            m_scale = Radius / 5f;
            ScaleParticleSystem(m_particleSystem, m_scale);
            transform.localScale = new Vector3(m_scale, m_scale, 1f);
        }

        // if server check for stun explosion
        if (IsServer)
        {
            DoBlindCheck();
        }
    }

    void DoBlindCheck()
    {
        // sync colliders to current transform
        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> enemyHitColliders = new List<Collider2D>();
        m_collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "EnemyHurt" }), enemyHitColliders);
        foreach (var hit in enemyHitColliders)
        {
            if (hit.HasComponent<NetworkCharacter>())
            {
                Debug.Log("Blind enemy at " + hit.transform.position + " for " + BlindDuration + " seconds.");
                var buffTimer = Instantiate(m_buffTimerPrefab);
                buffTimer.GetComponent<BuffTimer>().StartBuff(m_buffObject, hit.GetComponent<NetworkCharacter>(),
                    BlindDuration);
            }
        }
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        if (m_timer <= 0f && IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void ScaleParticleSystem(ParticleSystem particleSystem, float scale)
    {
        var mainModule = particleSystem.main;
        mainModule.startSizeMultiplier *= scale;
        mainModule.startSpeedMultiplier *= scale;
        mainModule.gravityModifierMultiplier *= scale;

        var emissionModule = particleSystem.emission;
        emissionModule.rateOverTimeMultiplier *= scale;
        emissionModule.rateOverDistanceMultiplier *= scale;

        var shapeModule = particleSystem.shape;
        shapeModule.scale *= scale;

        // Scale sub-emitters if they exist
        var subEmittersModule = particleSystem.subEmitters;
        for (int i = 0; i < subEmittersModule.subEmittersCount; i++)
        {
            ParticleSystem subEmitter = subEmittersModule.GetSubEmitterSystem(i);
            ScaleParticleSystem(subEmitter, scale);
        }
    }
}
