using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class BombSnail_Explode : EnemyAbility
{
    [Header("BombSnail_Explode Parameters")]
    public float ExplosionDuration = 1f;
    public float ExplosionRadius = 3f;

    [SerializeField] private Transform SpriteTransform;
    [SerializeField] private Collider2D Collider;

    private float m_explosionTimer = 0;

    public override void OnTelegraphStart()
    {
        transform.position = Parent.transform.position + new Vector3(0,0.3f,0);
        SpriteTransform.localScale = new Vector3(0f, 0f, 1);
        if (Parent != null) Parent.GetComponent<NetworkObject>().Despawn();

        GetComponentInChildren<FadeOut>().duration = ExplosionDuration;
    }

    public override void OnExecutionStart()
    {
        m_explosionTimer = 0f;
    }

    public override void OnUpdate()
    {
        m_explosionTimer += Time.deltaTime;
        if (m_explosionTimer > ExplosionDuration)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        m_explosionTimer = math.min(m_explosionTimer, ExplosionDuration);
        var alpha = m_explosionTimer / ExplosionDuration;

        SpriteTransform.localScale = new Vector3(alpha * ExplosionRadius, alpha * ExplosionRadius, 1);
    }
}
