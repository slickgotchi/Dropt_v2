using UnityEngine;

public class RechargeState : ShieldBlockState
{
    public RechargeState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine, Hand hand) : base(shieldBlock, shieldBlockStateMachine, hand)
    {
    }

    public override void Enter()
    {
        //m_shieldBlock.ShieldBarCanvasSetVisibleClientRpc(true);        
    }

    public override void Exit()
    {

    }

    public override void Update()
    {
        m_shieldBlock.RechargeHp(m_hand);
    }
}
