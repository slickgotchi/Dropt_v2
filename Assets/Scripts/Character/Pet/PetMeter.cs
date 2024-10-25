using UnityEngine;

public class PetMeter : MonoBehaviour
{
    [Tooltip("This is full charge amount required to summon a pet")]
    public float fullCharge = 100;
    [Tooltip("This is the amount each player hit will charge the PetMeter")]
    public float chargePerHit = 2;
    [Tooltip("This is the rate at which the meter drains during the charge up period")]
    public float chargeDrainPerSecond = 0.5f;
    [Tooltip("This is the length of time the pet summon will last")]
    public float summonDuration = 5;
    [Tooltip("This is the amount of time in seconds between each pet attack on an enemy")]
    public float attackInterval = 0.3f;
    [Tooltip("This is the amount of damage the pet does each time it attacks")]
    public float basePetAttackPower = 2.0f;

    private float m_currentChargeAmount;

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Recharge()
    {
        m_currentChargeAmount += chargePerHit;
        if (m_currentChargeAmount > fullCharge)
        {
            m_currentChargeAmount = fullCharge;
        }
    }

    public void Reset()
    {
        m_currentChargeAmount = 0;
    }

    internal float GetMeterValue()
    {
        return m_currentChargeAmount;
    }

    public void Drain()
    {
        m_currentChargeAmount -= chargeDrainPerSecond;
        if (m_currentChargeAmount < 0)
        {
            m_currentChargeAmount = 0;
        }
    }

    public bool IsFullyCharged()
    {
        return m_currentChargeAmount >= fullCharge;
    }

    public float GetProgress()
    {
        return m_currentChargeAmount / fullCharge;
    }
}