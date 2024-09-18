using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    [SerializeField] private ShieldBlockData m_shieldBlockData;
    private ShieldBlockStateMachine m_shieldBlockStateMachine;
    private ShieldBarCanvas m_shieldBarCanvas;
    private bool m_isInitialize;

    private ShieldDataContainer m_shieldDataContainer;

    public override void OnNetworkSpawn()
    {
        m_shieldBarCanvas = FindAnyObjectByType<ShieldBarCanvas>();
        m_shieldDataContainer = FindAnyObjectByType<ShieldDataContainer>();
    }

    public override void OnHoldStart()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (m_shieldDataContainer.HasShieldData(ActivationWearableNameEnum, AbilityHand))
        {
            ShieldData shieldData = m_shieldDataContainer.GetShieldData(ActivationWearableNameEnum, AbilityHand);
            m_shieldBlockData = shieldData.ShieldBlockData;
            m_shieldBlockStateMachine = shieldData.ShieldBlockStateMachine;
            ShieldBarCanvasSetVisible(IsActive());
        }
        else
        {
            if (IsServer)
            {
                m_shieldBlockData.Initialize(ActivationWearable.Rarity);
                m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
                m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
                ShieldData shieldData = new ShieldData(m_shieldBlockData, m_shieldBlockStateMachine);
                m_shieldDataContainer.SetShieldData(ActivationWearableNameEnum, AbilityHand, shieldData);
            }
        }

        SetProgress();
        //if (m_isInitialize)
        //{
        //    if (IsServer && m_shieldBlockData.IsActive())
        //    {
        //        m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
        //    }

        //    return;
        //}

        //m_isInitialize = true;
        //if (IsServer)
        //{
        //    m_shieldBlockData.Initialize(ActivationWearable.Rarity);
        //    m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
        //    m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
        //}
        //m_shieldBlockData.SubscribeOnHpValueChange(OnHpValueChange);
        //m_shieldBarCanvas.SetProgress(1);
        //PlayerHUDCanvas.Singleton.SetShieldBarProgress(ActivationInput.abilityHand, 1);
    }

    public override void OnHoldFinish()
    {
        if (IsServer && m_shieldBlockData.IsActive())
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
        }
    }

    public void ShieldBarCanvasSetVisible(bool visible)
    {
        m_shieldBarCanvas?.SetVisible(visible);
        PlayerHUDCanvas.Singleton.VisibleShieldBar(ActivationInput.abilityHand, visible);
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (IsActivated)
        {
            m_shieldBlockStateMachine?.Update();
        }
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
        PlayerHUDCanvas.Singleton.SetShieldBarProgress(ActivationInput.abilityHand, progress);
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