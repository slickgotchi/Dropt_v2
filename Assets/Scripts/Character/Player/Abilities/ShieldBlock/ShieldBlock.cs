using UnityEngine;

public class ShieldBlock : PlayerAbility
{
    [SerializeField] private ShieldBlockData m_shieldBlockData;
    private ShieldBlockStateMachine m_shieldBlockStateMachine;
    private ShieldBarCanvas m_shieldBarCanvas;

    public override void OnNetworkSpawn()
    {
        m_shieldBarCanvas = FindAnyObjectByType<ShieldBarCanvas>();
        //base.OnNetworkSpawn();
        //if (IsServer)
        //{
        //    Debug.Log("RARIRY :- " + ActivationWearable.Rarity);
        //    m_shieldBlockData.Initialize(ActivationWearable.Rarity);
        //    m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
        //    m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
        //}

        //m_shieldBlockData.SubscribeOnHpValueChange(OnHpValueChange);
    }

    public override void OnStart()
    {
        if (IsServer)
        {
            m_shieldBlockData.Initialize(ActivationWearable.Rarity);
            m_shieldBlockStateMachine = new ShieldBlockStateMachine(this);
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.InUseState);
        }
        m_shieldBlockData.SubscribeOnHpValueChange(OnHpValueChange);
        SetRotationToActionDirection();
        SetLocalPosition(PlayerAbilityCentreOffset + ActivationInput.actionDirection);
    }

    public void ShieldBarCanvasSetVisible(bool visible)
    {
        m_shieldBarCanvas?.SetVisible(visible);
        PlayerHUDCanvas.Singleton.VisibleShieldBar(ActivationInput.abilityHand, visible);
    }

    public override void OnFinish()
    {
        Debug.Log("FINISH SHIELD");
        m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
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
        float progress = m_shieldBlockData.GetHpRatio();
        m_shieldBarCanvas.SetProgress(m_shieldBlockData.GetHpRatio());
        PlayerHUDCanvas.Singleton.SetShieldBarProgress(ActivationInput.abilityHand, progress);

        if (newValue <= 0)
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.CoolDownState);
        }
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