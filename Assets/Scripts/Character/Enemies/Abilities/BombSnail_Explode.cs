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

    private void Awake()
    {
    }

    public override void OnActivate()
    {
        base.OnActivate();

        if (Parent == null) return;

        transform.position = Parent.transform.position;

        // resize explosion collider and check collisions
        Collider.GetComponent<CircleCollider2D>().radius = ExplosionRadius;
        var enemyCharacter = Parent.GetComponent<NetworkCharacter>();
        var damage = enemyCharacter.GetAttackPower();
        var isCritical = enemyCharacter.IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(Collider, damage, isCritical, enemyCharacter.gameObject);

        transform.parent = null;
        Parent.GetComponent<NetworkObject>().Despawn();

        // show a visual effect
        SpawnBasicCircleClientRpc(
            transform.position,
            Dropt.Utils.Color.HexToColor("#f5555d", 0.5f),
            ExplosionRadius);
    }
}
