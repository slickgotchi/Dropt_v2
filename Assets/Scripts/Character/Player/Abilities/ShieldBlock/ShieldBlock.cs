using UnityEngine;
using Unity.Netcode;

public class ShieldBlock : PlayerAbility
{
    private NetworkVariable<float> m_hp;

    [SerializeField] private ShieldBlockData m_shieldBlockData;

    private ShieldBlockStateMachine m_shieldBlockStateMachine;

    private bool m_isActive;

    public void Initialize(Wearable.RarityEnum rarityEnum)
    {
        m_hp = new NetworkVariable<float>(m_shieldBlockData.GetShieldBlockHp(rarityEnum));
        m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
        m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public bool IsActive()
    {
        return m_isActive;
    }

    public float AbsorbDamage(float damage)
    {
        if (m_hp.Value >= damage)
        {
            m_hp.Value -= damage;
            return 0;
        }
        else
        {
            //TODO :: break shield
            damage -= m_hp.Value;
            m_hp.Value = 0;
            return damage;
        }
    }
}