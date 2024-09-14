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

    // need to account for positions being delayed due to interpolation
    private float m_interpolationDelay = 0.3f;

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
        m_interpolationDelay = IsHost ? 0 : 3 * 1 / (float)NetworkManager.Singleton.NetworkTickSystem.TickRate;

        if (Parent == null) return;

        //transform.position = Parent.transform.position;
    }

    public override void OnTelegraphStart()
    {
        m_direction = AttackDirection;
        m_speed = StompDistance / ExecutionDuration;

        EnemyController.Facing facing = m_direction.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        if (Parent != null) Parent.GetComponent<EnemyController>().SetFacing(facing);
    }

    public override void OnExecutionStart()
    {
        if (Parent == null) return;

        m_isExecuting = true;

        Invoke("PlayJumpAnimation", m_interpolationDelay);
    }

    void PlayJumpAnimation()
    {
        if (Parent == null) return;
        Parent.GetComponent<Animator>().Play("Spider_Jump");
    }

    void PlayWalkAnimation()
    {
        if (Parent == null) return;
        Parent.GetComponent<Animator>().Play("Spider_Walk");
    }

    void SpawnStompCircle()
    {
        SpawnBasicCircleClientRpc(
            transform.position,
            Dropt.Utils.Color.HexToColor("#622461", 0.5f),
            1f);
    }

    public override void OnCooldownStart()
    {
        if (Parent == null) return;

        m_isExecuting = false;
        Invoke("PlayWalkAnimation", m_interpolationDelay);

        // do single collision check
        var damage = Parent.GetComponent<NetworkCharacter>().GetAttackPower();
        var isCritical = Parent.GetComponent<NetworkCharacter>().IsCriticalAttack();
        EnemyAbility.PlayerCollisionCheckAndDamage(m_collider, damage, isCritical);

        // activate the stomp circle
        Invoke("SpawnStompCircle", m_interpolationDelay);
    }

    public override void OnUpdate(float dt)
    {
        if (!m_isExecuting) return;

        transform.position += m_direction * m_speed * Time.deltaTime;
        if (Parent != null) Parent.transform.position = transform.position;
    }
}
