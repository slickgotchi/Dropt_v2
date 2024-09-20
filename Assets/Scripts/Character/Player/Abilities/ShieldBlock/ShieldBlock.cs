using Unity.Netcode;
using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    [SerializeField] private ShieldBlockData m_shieldBlockData;

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

    public override void OnHoldStart()
    {
        if (m_shieldDataContainer.HasShieldData(ActivationWearableNameEnum, AbilityHand))
        {
            ShieldData shieldData = m_shieldDataContainer.GetShieldData(ActivationWearableNameEnum, AbilityHand);
            m_shieldBlockData = shieldData.ShieldBlockData;
            m_hp.Value = m_shieldBlockData.RefHp;
            m_shieldBlockStateMachine = shieldData.ShieldBlockStateMachine;
            if (IsActive())
            {
                m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
            }
            PlayAnimation(IsActive() ? "ShieldBlock" : "ShieldDefault");
            ShieldBarCanvasSetVisibleClientRpc(IsActive());
        }
        else
        {
            m_shieldBlockData.Initialize(ActivationWearable.Rarity);
            m_hp.Value = m_shieldBlockData.TotalHp;
            SubscribeOnHpValueChangeClientRpc();
            m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
            ShieldData shieldData = new(m_shieldBlockData, m_shieldBlockStateMachine);
            m_shieldDataContainer.SetShieldData(ActivationWearableNameEnum, AbilityHand, shieldData);
        }
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection);
        SetInitialShieldProgressClientRpc();
    }

    public override void OnHoldFinish()
    {
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

    public void PlayAnimation(string animation)
    {
        base.PlayAnimation(animation);
    }

    public void ShieldBarCanvasSetVisible(bool visible)
    {
        m_shieldBarCanvas?.SetVisible(visible);
        PlayerHUDCanvas.Singleton.VisibleShieldBar(AbilityHand, visible);
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
        if (m_hp.Value >= m_shieldBlockData.TotalHp) return;

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
        m_shieldBlockData.SetRefHp(m_hp.Value);
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

    private float GetHpRatio()
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
        return m_shieldBlockData.GetCoolDownTime();
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
            //TODO :: break shield
            damage -= m_hp.Value;
            m_hp.Value = 0;
            return damage;
        }
    }
}