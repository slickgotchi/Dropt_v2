using UnityEngine;

public class InUseState : ShieldBlockState
{
    public InUseState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine) : base(shieldBlock, shieldBlockStateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("ABILITY IS IN USE");
        m_shieldBlock.ShieldBarCanvasSetVisible(true);
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        m_shieldBlock.DepleteShield();
    }
}
