using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Netcode.NetworkVariable<float>;

[Serializable]
public class ShieldBlockData
{
    [Serializable]
    public class BlockByRarity
    {
        public Wearable.RarityEnum Rarity;
        public float Hp;
    }

    [SerializeField] private List<BlockByRarity> m_blocksByRarity;
    [SerializeField] private float m_depletionRate;
    [SerializeField] private float m_rechargeRate;
    [SerializeField] private float m_breakCoolDown;

    private readonly NetworkVariable<float> m_hp = new NetworkVariable<float>(0);
    private float m_totalHp;
    private float m_rechargeAmountPerSecond;
    private float m_depletionAmountPerSecond;

    public void Initialize(Wearable.RarityEnum rarity)
    {
        m_hp.Value = m_blocksByRarity.Find(x => x.Rarity == rarity).Hp;
        m_totalHp = m_hp.Value;
        m_rechargeAmountPerSecond = m_totalHp * m_rechargeRate;
        m_depletionAmountPerSecond = m_totalHp * m_depletionRate;
    }

    public float GetCoolDownTime()
    {
        return m_breakCoolDown;
    }

    public void SubscribeOnHpValueChange(OnValueChangedDelegate onHpValueChange)
    {
        m_hp.OnValueChanged += onHpValueChange;
    }

    public void UnsubscribeOnHpValueChange(OnValueChangedDelegate onHpValueChange)
    {
        m_hp.OnValueChanged -= onHpValueChange;
    }

    public float GetShieldBlockHp(Wearable.RarityEnum rarity)
    {
        return m_blocksByRarity.Find(x => x.Rarity == rarity).Hp;
    }

    public void RechargeHp(float deltaTime)
    {
        if (m_hp.Value >= m_totalHp) return;

        float newHp = m_hp.Value + m_rechargeAmountPerSecond * deltaTime;
        if (newHp >= m_totalHp)
        {
            newHp = m_totalHp;
        }
        m_hp.Value = newHp;
    }

    public void DepleteShield(float deltaTime)
    {
        if (m_hp.Value <= 0)
        {
            return;
        }

        float newHp = m_hp.Value - m_depletionAmountPerSecond * deltaTime;
        if (newHp <= 0)
        {
            newHp = 0;
        }
        m_hp.Value = newHp;
    }

    public bool IsActive()
    {
        return m_hp.Value > 0;
    }

    public float GetHpRatio()
    {
        return m_hp.Value / m_totalHp;
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