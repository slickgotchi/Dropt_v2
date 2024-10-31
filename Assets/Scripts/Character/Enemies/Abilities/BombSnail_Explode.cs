using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class BombSnail_Explode : EnemyAbility
{
    [Header("BombSnail_Explode Parameters")]
    public float ExplosionDuration = 1f;

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
        var networkCharacter = Parent.GetComponent<NetworkCharacter>();
        var damage = networkCharacter.GetAttackPower();
        var isCritical = networkCharacter.IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(Collider, damage, isCritical, networkCharacter.gameObject);

        transform.parent = null;
        Parent.GetComponent<NetworkObject>().Despawn();
    }
}
