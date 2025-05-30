using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    private readonly Dictionary<Hand, ShieldData> m_shieldDatas = new Dictionary<Hand, ShieldData>();

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


    private readonly NetworkVariable<float> m_leftHp = new NetworkVariable<float>(0);
    private readonly NetworkVariable<float> m_rightHp = new NetworkVariable<float>(0);

    [SerializeField] private bool m_isBlocking;
    private Hand m_blockingHand;
    //private bool m_isOwner;

    [SerializeField] private ShieldBlockEffect _shieldBlockEffect;
    [SerializeField] private ShieldBarCanvas m_shieldBarCanvas;
    [SerializeField] private NetworkObject m_ownerNetworkObject;

    private void OnTransformParentChanged()
    {
        Transform parent = transform.parent;
        if (parent == null) return;

        //m_shieldBarCanvas = parent.GetComponentInChildren<ShieldBarCanvas>();
        //m_isOwner = parent.GetComponent<NetworkObject>().IsOwner;
        //_shieldBlockEffect = parent.GetComponentInChildren<ShieldBlockEffect>();
    }

    public void Initialize(Hand hand, Wearable.RarityEnum rarityEnum)
    {
        ShieldBlockData shieldBlockData;
        ShieldBlockStateMachine shieldBlockStateMachine;
        if (hand == Hand.Left)
        {
            m_leftHp.Value = m_blocksByRarity.Find(x => x.Rarity == rarityEnum).Hp;
            shieldBlockData = new ShieldBlockData(m_leftHp.Value, m_rechargeRate, m_depletionRate);
        }
        else
        {
            m_rightHp.Value = m_blocksByRarity.Find(x => x.Rarity == rarityEnum).Hp;
            shieldBlockData = new ShieldBlockData(m_rightHp.Value, m_rechargeRate, m_depletionRate);
        }

        shieldBlockStateMachine = new ShieldBlockStateMachine(this, hand);
        ShieldData shieldData = new(shieldBlockData, shieldBlockStateMachine);

        if (m_shieldDatas.ContainsKey(hand))
        {
            m_shieldDatas[hand] = shieldData;
        }
        else
        {
            m_shieldDatas.Add(hand, shieldData);
        }

        SubscribeOnHpValueChange(hand);
    }

    public override void OnHoldStart()
    {
        //HoldStartServerRpc(AbilityHand);
        if (!IsServer) return;
        if (m_isBlocking) return;

        ShieldBarCanvasSetVisibleClientRpc(true);

        if (!IsActive(AbilityHand)) return;
        ShieldBlockStateMachine shieldBlockStateMachine = m_shieldDatas[AbilityHand].ShieldBlockStateMachine;
        shieldBlockStateMachine.ChangeState(shieldBlockStateMachine.InUseState);
        float progress = GetHpRatio(AbilityHand);
        SetPlayerHudShieldProgressClientRpc(AbilityHand, progress, m_ownerNetworkObject.NetworkObjectId);
    }

    public override void OnHoldFinish()
    {
        //HoldFinishServerRpc(AbilityHand);
        if (!IsServer) return;
        if (m_isBlocking)
        {
            if (AbilityHand == m_blockingHand)
            {
                ShieldBarCanvasSetVisibleClientRpc(false);
            }
            ShieldBlockStateMachine shieldBlockStateMachine = m_shieldDatas[m_blockingHand].ShieldBlockStateMachine;
            shieldBlockStateMachine.ChangeState(shieldBlockStateMachine.RechargeState);
        }
        else
        {
            ShieldBarCanvasSetVisibleClientRpc(false);
        }
    }

    public void SubscribeOnHpValueChange(Hand hand)
    {
        if (hand == Hand.Left)
        {
            m_leftHp.OnValueChanged += OnLeftHpValueChange;
        }
        else
        {
            m_rightHp.OnValueChanged += OnRightHpValueChange;
        }
    }

    public void UnsubscribeOnHpValueChange(Hand hand)
    {
        if (hand == Hand.Left)
        {
            m_leftHp.OnValueChanged -= OnLeftHpValueChange;
        }
        else
        {
            m_rightHp.OnValueChanged -= OnRightHpValueChange;
        }
    }

    [ClientRpc]
    public void SetPlayerHudShieldProgressClientRpc(Hand hand, float progress, ulong ownerNetworkObjectId)
    {
        if (m_ownerNetworkObject.NetworkObjectId != ownerNetworkObjectId)
        {
            return;
        }

        PlayerHUDCanvas.Instance.SetShieldBarProgress(hand, progress);
        _shieldBlockEffect?.UpdateEffect(progress);
    }

    [ClientRpc]
    public void ShieldBarCanvasSetVisibleClientRpc(bool visible)
    {
        ShieldBarCanvasSetVisible(visible);
        _shieldBlockEffect.SetVisible(visible);
    }

    public void ShieldBarCanvasSetVisible(bool visible)
    {
        m_shieldBarCanvas?.SetVisible(visible);
    }

    protected override void Update()
    {
        base.Update();

        if (!IsServer)
        {
            return;
        }

        foreach (KeyValuePair<Hand, ShieldData> data in m_shieldDatas)
        {
            data.Value.ShieldBlockStateMachine?.Update();
        }

        if (m_isBlocking)
        {
            float progress = GetHpRatio(m_blockingHand);
            SetShieldBarCanvasProgressClientRpc(progress);
        }
    }

    [ClientRpc]
    private void SetShieldBarCanvasProgressClientRpc(float progress)
    {
        m_shieldBarCanvas.SetProgress(progress);
    }

    public void RechargeHp(Hand hand)
    {
        ShieldData shieldData = m_shieldDatas[hand];
        if (hand == Hand.Left)
        {
            if (m_leftHp.Value >= shieldData.ShieldBlockData.TotalHp)
            {
                return;
            }

            float newHp = m_leftHp.Value + (shieldData.ShieldBlockData.RechargeAmountPerSecond * Time.deltaTime);
            if (newHp >= shieldData.ShieldBlockData.TotalHp)
            {
                newHp = shieldData.ShieldBlockData.TotalHp;
            }
            m_leftHp.Value = newHp;
        }
        else
        {
            if (m_rightHp.Value >= shieldData.ShieldBlockData.TotalHp)
            {
                return;
            }

            float newHp = m_rightHp.Value + (shieldData.ShieldBlockData.RechargeAmountPerSecond * Time.deltaTime);
            if (newHp >= shieldData.ShieldBlockData.TotalHp)
            {
                newHp = shieldData.ShieldBlockData.TotalHp;
            }
            m_rightHp.Value = newHp;
        }
    }

    private void OnLeftHpValueChange(float previousValue, float newValue)
    {
        SetPlayerHudShieldProgressClientRpc(Hand.Left, GetHpRatio(Hand.Left), m_ownerNetworkObject.NetworkObjectId);
        if (newValue <= 0)
        {
            ShieldBlockStateMachine shieldBlockStateMachine = m_shieldDatas[Hand.Left].ShieldBlockStateMachine;
            shieldBlockStateMachine.ChangeState(shieldBlockStateMachine.CoolDownState);
        }
    }

    private void OnRightHpValueChange(float previousValue, float newValue)
    {
        SetPlayerHudShieldProgressClientRpc(Hand.Right, GetHpRatio(Hand.Right), m_ownerNetworkObject.NetworkObjectId);
        if (newValue <= 0)
        {
            ShieldBlockStateMachine shieldBlockStateMachine = m_shieldDatas[Hand.Right].ShieldBlockStateMachine;
            shieldBlockStateMachine.ChangeState(shieldBlockStateMachine.CoolDownState);
        }
    }

    public float GetHpRatio(Hand hand)
    {
        ShieldBlockData shieldBlockData = m_shieldDatas[hand].ShieldBlockData;
        return hand == Hand.Left ? m_leftHp.Value / shieldBlockData.TotalHp : m_rightHp.Value / shieldBlockData.TotalHp;
    }

    public void StartBlocking(Hand hand)
    {
        m_isBlocking = true;
        m_blockingHand = hand;
    }

    public void StopBlocking(Hand hand)
    {
        if (hand == m_blockingHand)
        {
            m_isBlocking = false;
        }
    }

    public bool IsBlocking()
    {
        return m_isBlocking;
    }

    public void DepleteShield(Hand hand)
    {
        ShieldBlockData shieldBlockData = m_shieldDatas[hand].ShieldBlockData;
        if (hand == Hand.Left)
        {
            if (m_leftHp.Value <= 0)
            {
                return;
            }

            float newHp = m_leftHp.Value - (shieldBlockData.DepletionAmountPerSecond * Time.deltaTime);
            if (newHp <= 0)
            {
                newHp = 0;
            }
            m_leftHp.Value = newHp;
        }
        else
        {
            if (m_rightHp.Value <= 0)
            {
                return;
            }

            float newHp = m_rightHp.Value - (shieldBlockData.DepletionAmountPerSecond * Time.deltaTime);
            if (newHp <= 0)
            {
                newHp = 0;
            }
            m_rightHp.Value = newHp;
        }
    }

    public float GetCoolDownTime()
    {
        return m_breakCoolDown;
    }

    public bool IsActive(Hand hand)
    {
        return (hand == Hand.Left && m_leftHp.Value > 0) || (hand == Hand.Right && m_rightHp.Value > 0);
    }

    public float AbsorbDamage(float damage)
    {
        if (m_blockingHand == Hand.Left)
        {
            if (m_leftHp.Value >= damage)
            {
                m_leftHp.Value -= damage;
                return 0;
            }
            else
            {
                damage -= m_leftHp.Value;
                m_leftHp.Value = 0;
                return damage;
            }
        }
        else
        {
            if (m_rightHp.Value >= damage)
            {
                m_rightHp.Value -= damage;
                return 0;
            }
            else
            {
                damage -= m_rightHp.Value;
                m_rightHp.Value = 0;
                return damage;
            }
        }
    }

    public void Deactivate(Hand hand)
    {
        if (AbilityHand != hand)
        {
            return;
        }

        if (m_shieldDatas.ContainsKey(hand))
        {
            m_shieldDatas.Remove(hand);
        }

        UnsubscribeOnHpValueChange(hand);
    }
}