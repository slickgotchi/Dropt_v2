using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyAbility : NetworkBehaviour
{
    public float TelegraphDuration = 1f;
    public float ExecutionDuration = 1f;
    public float CooldownDuration = 1f;
    public GameObject Parent;
    public GameObject Target;

    private float m_timer = 0;

    public enum State
    {
        None, Telegraph, Execution, Cooldown,
    }
    public State EnemyAbilityState = State.None;

    public override void OnNetworkSpawn()
    {
    }

    public void Activate()
    {
        m_timer = TelegraphDuration;
        EnemyAbilityState = State.Telegraph;
        OnTelegraphStart();
    }

    private void Update()
    {
        m_timer -= Time.deltaTime;

        switch (EnemyAbilityState)
        {
            case State.Telegraph:
                if (m_timer <= 0)
                {
                    m_timer = ExecutionDuration;
                    EnemyAbilityState = State.Execution;
                    OnExecutionStart();
                }
                break;
            case State.Execution:
                if (m_timer <= 0)
                {
                    m_timer = CooldownDuration;
                    EnemyAbilityState = State.Cooldown;
                    OnCooldownStart();
                }
                break;
            case State.Cooldown:
                if (m_timer <= 0)
                {
                    m_timer = 0;
                    EnemyAbilityState = State.None;
                    OnFinish();
                    GetComponent<NetworkObject>().Despawn();
                }
                break;
            case State.None: break;
            default: break;
        }

        OnUpdate();
    }

    public virtual void OnTelegraphStart() { }
    public virtual void OnExecutionStart() { }
    public virtual void OnCooldownStart() { }
    public virtual void OnFinish() { }
    public virtual void OnUpdate() { }
}
