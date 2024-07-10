using CarlosLab.UtilityIntelligence;
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

    private float m_explosionGrowFadeTimer = 0;

    private void Awake()
    {
        SpriteTransform.localScale = Vector3.one;
    }

    public override void OnTelegraphStart()
    {
        Debug.Log("OnTelegraphStart()");
    }

    public override void OnExecutionStart()
    {
        Debug.Log("OnExecutionStart()");

        // reset explosion fade timer & set fade out duration
        m_explosionGrowFadeTimer = 0f;
        GetComponentInChildren<FadeOut>().duration = ExplosionDuration;

        // set position of explosion and initial scale
        transform.position = Parent.transform.position + new Vector3(0, 0.3f, 0);
        SpriteTransform.localScale = new Vector3(0f, 0f, 1);

        // resize explosion collider and check collisions
        Collider.GetComponent<CircleCollider2D>().radius = ExplosionRadius;
        var enemyCharacter = Parent.GetComponent<NetworkCharacter>();
        var damage = enemyCharacter.GetAttackPower();
        var isCritical = enemyCharacter.IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(Collider, damage, isCritical, enemyCharacter.gameObject);

        // destroy the parent object
        if (Parent != null)
        {
            transform.parent = null;
            Parent.GetComponent<UtilityAgentFacade>().Destroy();
        }
    }

    public override void OnUpdate()
    {
        m_explosionGrowFadeTimer += Time.deltaTime;
        if (m_explosionGrowFadeTimer > ExplosionDuration)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        m_explosionGrowFadeTimer = math.min(m_explosionGrowFadeTimer, ExplosionDuration);
        var alpha = m_explosionGrowFadeTimer / ExplosionDuration;

        SpriteTransform.localScale = new Vector3(alpha * ExplosionRadius, alpha * ExplosionRadius, 1);
    }
}
