using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class BombItem : NetworkBehaviour
{
    [SerializeField] private float m_radius;
    private readonly NetworkVariable<int> m_timer = new NetworkVariable<int>(3);
    private SoundFX_BombItem m_soundFX_BombItem;
    [SerializeField] private GameObject m_explosionEffect;
    [SerializeField] private LayerMask m_destructibleLayer;
    [SerializeField] private GameObject m_body;
    [SerializeField] private bool m_showBombRadius = true;
    private LineRenderer lineRenderer;
    public int segments = 100;

    public override async void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_soundFX_BombItem = GetComponent<SoundFX_BombItem>();
        UpdateTimerText(m_timer.Value);
        m_timer.OnValueChanged += OnTimerValueChanged;
        InitializeBombRadius();

        if (IsServer)
        {
            await StartTimer();
            m_body.SetActive(false);
            SpawnExplosionEffectClientRpc();
            DetactAndDestroyDestructible();
            await UniTask.Delay(1000);
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void InitializeBombRadius()
    {
        if (!m_showBombRadius) return;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1; // Add one to close the circle
        lineRenderer.loop = true; // Close the circle
        lineRenderer.useWorldSpace = false;
        DrawCircleShape();
    }

    private void DrawCircleShape()
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            points[i] = new Vector3(Mathf.Cos(angle) * m_radius, Mathf.Sin(angle) * m_radius, 0f);
        }
        points[segments] = points[0]; // Close the loop
        lineRenderer.SetPositions(points);
    }

    private void OnTimerValueChanged(int previousValue, int newValue)
    {
        UpdateTimerText(newValue);
    }

    private void UpdateTimerText(int timer)
    {
        Transform myTransform = transform;
        Vector3 textposition = new Vector3(myTransform.position.x,
                                           myTransform.position.y + 0.5f,
                                           myTransform.position.z);
        PopupTextManager.Instance.PopupText(timer.ToString(),
                                            textposition,
                                            20,
                                            Color.white);
        m_soundFX_BombItem.PlayTimerSound();
    }

    private async UniTask StartTimer()
    {
        await UniTask.Delay(1000);
        while (m_timer.Value > 0)
        {
            m_timer.Value--;
            await UniTask.Delay(1000);
        }
    }

    [ClientRpc]
    private void SpawnExplosionEffectClientRpc()
    {
        ParticleSystem particleSystem = m_explosionEffect.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startSize = m_radius * 2.5f;
        _ = Instantiate(m_explosionEffect, transform.position, Quaternion.identity);
        m_soundFX_BombItem.PlayExplosionSound();
    }

    private void DetactAndDestroyDestructible()
    {
        Collider2D[] destructibleObjects = Physics2D.OverlapCircleAll(transform.position, m_radius, m_destructibleLayer);
        foreach (Collider2D destructibleObject in destructibleObjects)
        {
            Destructible destructible = destructibleObject.GetComponent<Destructible>();
            destructible.Explode();
        }
    }
}