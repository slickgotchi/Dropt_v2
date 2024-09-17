using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShieldBlockData
{
    [System.Serializable]
    public class BlockByRarity
    {
        public Wearable.RarityEnum Rarity;
        public float Hp;
    }

    [SerializeField] private List<BlockByRarity> m_blocksByRarity;
    [SerializeField] private float m_depletionRate;
    [SerializeField] private float m_rechargeRate;
    [SerializeField] private float m_breakCoolDown;

    public float GetShieldBlockHp(Wearable.RarityEnum rarity)
    {
        return m_blocksByRarity.Find(x => x.Rarity == rarity).Hp;
    }
}