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

    //[SerializeField] private Transform SpriteTransform;
    [SerializeField] private Collider2D Collider;

    private float m_explosionTimer = 0;

    private void Awake()
    {
        //SpriteTransform.localScale = Vector3.one;
    }

    public override void OnTelegraphStart()
    {
        Debug.Log("OnTelegraphStart()");
    }

    public override void OnExecutionStart()
    {
        Debug.Log("OnExecutionStart()");

        // reset explosion fade timer & set fade out duration
        m_explosionTimer = 0f;

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

        // show a visual effect
        SpawnBasicCircleClientRpc(
            transform.position,
            Dropt.Utils.Color.HexToColor("#f5555d", 0.5f),
            ExplosionRadius);
    }

    

    public override void OnUpdate()
    {
        m_explosionTimer += Time.deltaTime;
        if (m_explosionTimer > ExplosionDuration)
        {
            if (IsServer) GetComponent<NetworkObject>().Despawn();
            return;
        }
    }
}
