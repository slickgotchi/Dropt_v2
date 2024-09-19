using Unity.Netcode;
using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    [SerializeField] private ShieldBlockData m_shieldBlockData;

    private ShieldBlockStateMachine m_shieldBlockStateMachine;
    private ShieldBarCanvas m_shieldBarCanvas;
    private ShieldDataContainer m_shieldDataContainer;

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
            SubscribeOnHpValueChangeClientRpc();
            m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
            ShieldData shieldData = new ShieldData(m_shieldBlockData, m_shieldBlockStateMachine);
            m_shieldDataContainer.SetShieldData(ActivationWearableNameEnum, AbilityHand, shieldData);
        }
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection);
        SetInitialShieldProgressClientRpc();
    }

    public override void OnHoldFinish()
    {
        if (m_shieldBlockData.IsActive())
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
        }
    }

    [ClientRpc]
    public void SubscribeOnHpValueChangeClientRpc()
    {
        m_shieldBlockData.SubscribeOnHpValueChange(OnHpValueChange);
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
        m_shieldBlockData.RechargeHp(Time.deltaTime);
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
        float progress = m_shieldBlockData.GetHpRatio();
        m_shieldBarCanvas.SetProgress(m_shieldBlockData.GetHpRatio());
        PlayerHUDCanvas.Singleton.SetShieldBarProgress(AbilityHand, progress);
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
        m_shieldBlockData.DepleteShield(Time.deltaTime);
    }

    public float GetCoolDownTime()
    {
        return m_shieldBlockData.GetCoolDownTime();
    }

    public bool IsActive()
    {
        return m_shieldBlockData.IsActive();
    }

    public float AbsorbDamage(float damage)
    {
        return m_shieldBlockData.AbsorbDamage(damage);
    }
}