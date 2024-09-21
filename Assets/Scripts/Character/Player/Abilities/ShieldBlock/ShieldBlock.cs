using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    private ShieldBlockData m_shieldBlockData;

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

    private ShieldBlockStateMachine m_shieldBlockStateMachine;
    private ShieldBarCanvas m_shieldBarCanvas;
    private ShieldDataContainer m_shieldDataContainer;

    private readonly NetworkVariable<float> m_hp = new NetworkVariable<float>(0);

    private void OnTransformParentChanged()
    {
        Transform parent = transform.parent;
        m_shieldBarCanvas = parent.GetComponentInChildren<ShieldBarCanvas>();
        m_shieldDataContainer = parent.GetComponentInChildren<ShieldDataContainer>();
    }

    public void Initialize(Wearable.NameEnum nameEnum, Hand hand, Wearable.RarityEnum rarityEnum)
    {
        if (m_shieldDataContainer.HasShieldData(nameEnum, hand))
        {
            ShieldData shieldData = m_shieldDataContainer.GetShieldData(nameEnum, hand);
            m_shieldBlockData = shieldData.ShieldBlockData;
            m_hp.Value = m_shieldBlockData.RefHp;
            m_shieldBlockStateMachine = shieldData.ShieldBlockStateMachine;
        }
        else
        {
            m_hp.Value = m_blocksByRarity.Find(x => x.Rarity == rarityEnum).Hp;
            m_shieldBlockData = new ShieldBlockData(m_hp.Value, m_rechargeRate, m_depletionRate);
            SubscribeOnHpValueChangeClientRpc();
            m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
            ShieldData shieldData = new(m_shieldBlockData, m_shieldBlockStateMachine);
            m_shieldDataContainer.SetShieldData(nameEnum, hand, shieldData);
        }
    }

    public override void OnHoldStart()
    {
        ShieldBarCanvasSetVisibleClientRpc(true);
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection);
        SetInitialShieldProgressClientRpc();
        if (IsActive())
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
        }
    }

    public override void OnHoldFinish()
    {
        ShieldBarCanvasSetVisibleClientRpc(false);
        if (IsActive())
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
        }
    }

    [ClientRpc]
    public void SubscribeOnHpValueChangeClientRpc()
    {
        m_hp.OnValueChanged += OnHpValueChange;
    }

    [ClientRpc]
    public void SetInitialShieldProgressClientRpc()
    {
        SetProgress();
    }

    [ClientRpc]
    public void ShieldBarCanvasSetVisibleClientRpc(bool visible)
    {
        ShieldBarCanvasSetVisible(visible);
    }

    public void ShieldBarCanvasSetVisible(bool visible)
    {
        m_shieldBarCanvas?.SetVisible(visible);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        m_shieldBlockStateMachine?.Update();
    }

    public void RechargeHp()
    {
        if (m_hp.Value >= m_shieldBlockData.TotalHp)
        {
            return;
        }

        float newHp = m_hp.Value + (m_shieldBlockData.RechargeAmountPerSecond * Time.deltaTime);
        if (newHp >= m_shieldBlockData.TotalHp)
        {
            newHp = m_shieldBlockData.TotalHp;
        }
        m_hp.Value = newHp;
    }

    private void OnHpValueChange(float previousValue, float newValue)
    {
        SetProgress();
        if (newValue <= 0)
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.CoolDownState);
        }
    }

    private void SetProgress()
    {
        float progress = GetHpRatio();
        m_shieldBarCanvas.SetProgress(progress);
        PlayerHUDCanvas.Singleton.SetShieldBarProgress(AbilityHand, progress);
    }

    public float GetHpRatio()
    {
        return m_hp.Value / m_shieldBlockData.TotalHp;
    }

    public void StartBlocking()
    {
        m_shieldBlockData.StartBlocking();
    }

    public void StopBlocking()
    {
        m_shieldBlockData.StopBlocking();
    }

    public bool IsBlocking()
    {
        return m_shieldBlockData.IsBlocking();
    }

    public void DepleteShield()
    {
        if (m_hp.Value <= 0)
        {
            return;
        }

        float newHp = m_hp.Value - (m_shieldBlockData.DepletionAmountPerSecond * Time.deltaTime);
        if (newHp <= 0)
        {
            newHp = 0;
        }
        m_hp.Value = newHp;
    }

    public float GetCoolDownTime()
    {
        return m_breakCoolDown;
    }

    public bool IsActive()
    {
        return m_hp.Value > 0;
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
            damage -= m_hp.Value;
            m_hp.Value = 0;
            return damage;
        }
    }

    public void Deactivate(Hand hand)
    {
        if (AbilityHand != hand)
        {
            return;
        }
        m_shieldBlockData?.SetRefHp(m_hp.Value);
        m_shieldBlockStateMachine = null;
    }
}