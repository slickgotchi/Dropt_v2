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

    public override void OnNetworkSpawn()
    {

    }

    public override void OnInit()
    {
        
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
        //m_animator.Play("SnailSlash_Attack");
        Dropt.Utils.Anim.PlayAnimationWithDuration(m_animator, "SnailSlash_Attack", Parent.GetComponent<Dropt.EnemyAI>().AttackDuration);
    }

    public override void OnTelegraphStart()
    {
        //if (Parent == null) return;

        //if (Parent.HasComponent<Animator>())
        //{
        //    Parent.GetComponent<Animator>().Play("Snail_TelegraphAttack");
        //}

        //// setup attack
        //Vector3 attackDir = (Target.transform.position - Parent.transform.position).normalized;
        ////transform.rotation = PlayerAbility.GetRotationFromDirection(attackDir);

        //EnemyController.Facing facing = attackDir.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        //Parent.GetComponent<EnemyController>().SetFacingDirection(facing, 1f);
    }

    public override void OnExecutionStart()
    {
        if (Parent == null) return;

        //transform.position = Parent.transform.position + new Vector3(0, 0.35f, 0f);
        m_animator.Play("SnailSlash_Attack");
        var damage = Parent.GetComponent<NetworkCharacter>().GetAttackPower();
        var isCritical = Parent.GetComponent<NetworkCharacter>().IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical, Parent);
    }

    public override void OnCooldownStart()
    {
    }

    public override void OnFinish()
    {
    }
}
