using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spider_Stomp : EnemyAbility
{
    [Header("Spider_Stomp Parameters")]
    public float StompDistance = 2f;

    private Vector3 m_direction;
    private float m_speed;
    private Collider2D m_collider;
    private bool m_isExecuting = false;
    private GameObject m_stompCircle;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
        m_stompCircle = transform.Find("StompCircle").gameObject;
        m_stompCircle.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        transform.position = Parent.transform.position;
    }

    public override void OnTelegraphStart()
    {
        m_direction = (Target.transform.position - Parent.transform.position).normalized;
        m_speed = StompDistance / ExecutionDuration;

        EnemyController.Facing facing = m_direction.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        if (Parent != null) Parent.GetComponent<EnemyController>().SetFacingDirection(facing);
    }

    public override void OnExecutionStart()
    {
        m_isExecuting = true;
        Parent.GetComponent<Animator>().Play("Spider_Jump");
    }

    public override void OnCooldownStart()
    {
        m_isExecuting = false;
        Parent.GetComponent<Animator>().Play("Spider_Walk");

        // do single collision check
        var damage = Parent.GetComponent<NetworkCharacter>().GetAttackPower();
        var isCritical = Parent.GetComponent<NetworkCharacter>().IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical);

        // activate the stomp circle
        m_stompCircle.SetActive(true);
        m_stompCircle.transform.position = transform.position;
    }

    public override void OnUpdate()
    {
        if (!m_isExecuting) return;

        transform.position += m_direction * m_speed * Time.deltaTime;
        if (Parent != null) Parent.transform.position = transform.position;
    }
}
