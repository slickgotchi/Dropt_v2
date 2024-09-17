public abstract class ShieldBlockState
{
    protected ShieldBlock m_shieldBlock;
    protected ShieldBlockStateMachine m_shieldBlockStateMachine;

    public ShieldBlockState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine)
    {
        m_shieldBlock = shieldBlock;
        m_shieldBlockStateMachine = shieldBlockStateMachine;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
