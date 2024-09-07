using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Snail_Slash : EnemyAbility
{
    private Animator m_animator;
    private Collider2D m_collider;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_collider = GetComponentInChildren<Collider2D>();
    }

    public override void OnActivate()
    {
        if (Parent == null) return;

        // get direction and parent centre position
        var dir = Parent.GetComponent<Dropt.EnemyAI>().AttackDirection;
        var parentCentre = Parent.GetComponentInChildren<AttackCentre>();
        var parentCentrePos = parentCentre == null ? Parent.transform.position : parentCentre.transform.position;

        // set rotation
        transform.rotation = PlayerAbility.GetRotationFromDirection(dir);

        // set offset
        transform.position = parentCentrePos + dir * 0.5f;

        // play animation
        Dropt.Utils.Anim.PlayAnimationWithDuration(m_animator, "SnailSlash_Attack", Parent.GetComponent<Dropt.EnemyAI>().AttackDuration);

        // do damage
        var damage = Parent.GetComponent<NetworkCharacter>().GetAttackPower();
        var isCritical = Parent.GetComponent<NetworkCharacter>().IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical, Parent);
    }
}
