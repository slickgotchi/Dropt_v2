using System;

[Serializable]
public class ShieldBlockData
{
    public float TotalHp { get; private set; }
    public float RefHp { get; private set; }
    public float RechargeAmountPerSecond { get; private set; }
    public float DepletionAmountPerSecond { get; private set; }
    private bool m_isBlocking;

    public ShieldBlockData(float hp, float rechargeRate, float deplationRate)
    {
        TotalHp = hp;
        RechargeAmountPerSecond = TotalHp * rechargeRate;
        DepletionAmountPerSecond = TotalHp * deplationRate;
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