using UnityEngine;

public class InUseState : ShieldBlockState
{
    public InUseState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine) : base(shieldBlock, shieldBlockStateMachine)
    {
    }

    public override void Enter()
    {
        m_shieldBlock.ShieldBarCanvasSetVisibleClientRpc(true);
        m_shieldBlock.PlayAnimation("ShieldBlock");
        m_shieldBlock.StartBlocking();
    }

    public override void Exit()
    {
        m_shieldBlock.PlayAnimation("ShieldDefault");
        m_shieldBlock.StopBlocking();
    }

    public override void Update()
    {
        m_shieldBlock.DepleteShield();
    }
}
