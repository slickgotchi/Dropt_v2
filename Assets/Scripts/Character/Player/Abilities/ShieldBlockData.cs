using System;
using System.Collections.Generic;
using UnityEngine;

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

    public float TotalHp { get; private set; }
    public float RefHp { get; private set; }
    public float RechargeAmountPerSecond { get; private set; }
    public float DepletionAmountPerSecond { get; private set; }
    private bool m_isBlocking;

    public void Initialize(Wearable.RarityEnum rarity)
    {
        TotalHp = m_blocksByRarity.Find(x => x.Rarity == rarity).Hp;
        RechargeAmountPerSecond = TotalHp * m_rechargeRate;
        DepletionAmountPerSecond = TotalHp * m_depletionRate;
    }

    public float GetCoolDownTime()
    {
        return m_breakCoolDown;
    }

    public float GetShieldBlockHp(Wearable.RarityEnum rarity)
    {
        return m_blocksByRarity.Find(x => x.Rarity == rarity).Hp;
    }

    public void StartBlocking()
    {
        m_isBlocking = true;
    }

    public void StopBlocking()
    {
        m_isBlocking = false;
    }

    public bool IsBlocking()
    {
        return m_isBlocking;
    }

    public void SetRefHp(float hp)
    {
        RefHp = hp;
    }
}