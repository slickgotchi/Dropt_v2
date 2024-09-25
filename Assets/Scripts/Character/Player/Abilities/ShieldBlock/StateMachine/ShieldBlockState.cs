public abstract class ShieldBlockState
{
    protected ShieldBlock m_shieldBlock;
    protected ShieldBlockStateMachine m_shieldBlockStateMachine;
    protected Hand m_hand;
    private ShieldBlock shieldBlock;
    private ShieldBlockStateMachine shieldBlockStateMachine;

    public ShieldBlockState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine, Hand hand)
    {
        m_hand = hand;
        m_shieldBlock = shieldBlock;
        m_shieldBlockStateMachine = shieldBlockStateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
