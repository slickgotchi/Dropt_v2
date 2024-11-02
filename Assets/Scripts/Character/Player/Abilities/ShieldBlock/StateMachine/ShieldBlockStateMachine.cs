public class ShieldBlockStateMachine
{
    private ShieldBlockState m_currentState;

    public readonly RechargeState RechargeState;
    public readonly CoolDownState CoolDownState;
    public readonly InUseState InUseState;
    //public readonly Hand Hand;

    public ShieldBlockStateMachine(ShieldBlock shieldBlock, Hand hand)
    {
        //hand = Hand;
        RechargeState = new RechargeState(shieldBlock, this, hand);
        CoolDownState = new CoolDownState(shieldBlock, this, hand);
        InUseState = new InUseState(shieldBlock, this, hand);
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
