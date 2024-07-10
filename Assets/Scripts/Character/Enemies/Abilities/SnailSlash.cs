using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SnailSlash : EnemyAbility
{
    private Animator m_animator;
    private Collider2D m_collider;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponentInChildren<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {

    }

    public override void OnTelegraphStart()
    {
        // check parent
        if (Parent == null)
        {
            Debug.Log("No parent for SnailSlash!");
            return;
        }

        if (Parent.HasComponent<Animator>())
        {
            Parent.GetComponent<Animator>().Play("Snail_TelegraphAttack");
        }

        // setup attack
        Vector3 attackDir = (Target.transform.position - Parent.transform.position).normalized;
        transform.rotation = PlayerAbility.GetRotationFromDirection(attackDir);

        EnemyController.Facing facing = attackDir.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        Parent.GetComponent<EnemyController>().SetFacingDirection(facing, 1f);
    }

    public override void OnExecutionStart()
    {
        transform.position = Parent.transform.position + new Vector3(0, 0.35f, 0f);
        m_animator.Play("SnailSlash_Attack");
        var damage = Parent.GetComponent<NetworkCharacter>().GetAttackPower();
        var isCritical = Parent.GetComponent<NetworkCharacter>().IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical, Parent);
        //CollisionCheck();
    }

    public override void OnCooldownStart()
    {
    }

    public override void OnFinish()
    {
    }

    //void CollisionCheck()
    //{
    //    // sync colliders to current transform
    //    Physics2D.SyncTransforms();

    //    // do a collision check
    //    List<Collider2D> playerHitColliders = new List<Collider2D>();
    //    m_collider.Overlap(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
    //    foreach (var hit in playerHitColliders)
    //    {
    //        var player = hit.transform.parent;
    //        if (player.HasComponent<NetworkCharacter>())
    //        {
    //            player.GetComponent<NetworkCharacter>().TakeDamage(10, false, gameObject);
    //        }
    //    }

    //    // clear out colliders
    //    playerHitColliders.Clear();
    //}
}
