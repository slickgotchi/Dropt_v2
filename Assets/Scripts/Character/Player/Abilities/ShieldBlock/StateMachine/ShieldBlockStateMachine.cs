public class ShieldBlockStateMachine
{
    private ShieldBlockState m_currentState;

    public readonly RechargeState RechargeState;
    public readonly CoolDownState CoolDownState;
    public readonly InUseState InUseState;

    public ShieldBlockStateMachine(ShieldBlock shieldBlock)
    {
        RechargeState = new RechargeState(shieldBlock, this);
        CoolDownState = new CoolDownState(shieldBlock, this);
        InUseState = new InUseState(shieldBlock, this);
    }

    public void ChangeState(ShieldBlockState shieldBlockState)
    {
        m_currentState?.Exit();
        m_currentState = shieldBlockState;
        m_currentState.Enter();
    }

    public void Update()
    {
        m_currentState?.Update();
    }
}
