using UnityEngine;

public class InUseState : ShieldBlockState
{
    public InUseState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine, Hand hand) : base(shieldBlock, shieldBlockStateMachine, hand)
    {
    }

    public override void Enter()
    {
        //m_shieldBlock.ShieldBarCanvasSetVisibleClientRpc(true);
        //m_shieldBlock.PlayAnimation("ShieldBlock");
        m_shieldBlock.StartBlocking(m_hand);
    }

    public override void Exit()
    {
        //m_shieldBlock.PlayAnimation("ShieldDefault");
        m_shieldBlock.StopBlocking(m_hand);
    }

    public override void Update()
    {
        m_shieldBlock.DepleteShield(m_hand);
    }
}
