using UnityEngine;

public class CoolDownState : ShieldBlockState
{
    private float m_coolDownTime;
    private float m_timer;

    public CoolDownState(ShieldBlock shieldBlock, ShieldBlockStateMachine shieldBlockStateMachine) : base(shieldBlock, shieldBlockStateMachine)
    {
    }

    public override void Enter()
    {
        m_coolDownTime = m_shieldBlock.GetCoolDownTime();
        m_timer = 0;
        //m_shieldBlock.ShieldBarCanvasSetVisibleClientRpc(false);
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer >= m_coolDownTime)
        {
            m_shieldBlockStateMachine.ChangeState(m_shieldBlockStateMachine.RechargeState);
        }
    }
}
