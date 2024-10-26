using UnityEngine;

public class PetAttackState : PetState
{
    private float m_attackInterval;
    private float m_counter;
    private Transform m_previousEnemy;
    private float m_summonDuration;
    private float m_summonCounter;

    public PetAttackState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {
    }

    public override void Enter()
    {
        m_PetController.ActivatePetViewClientRpc();
        m_attackInterval = m_PetController.GetAttackInterval();
        m_summonCounter = m_summonDuration = m_PetController.GetSummonDuration();
    }

    public override void Exit()
    {
        m_counter = 0;
    }

    public override void Update()
    {
        m_counter += Time.deltaTime;
        m_summonCounter -= Time.deltaTime;
        m_PetController.SetSummonProgress(m_summonCounter / m_summonDuration);
        if (m_summonCounter <= 0)
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetDeactivateState);
            return;
        }

        if (m_counter < m_attackInterval)
        {
            return;
        }

        m_counter = 0;
        Transform enemyTransform = m_PetController.GetEnemyInPlayerRange(m_previousEnemy);
        if (enemyTransform == null)
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetFollowOwnerState);
            return;
        }
        m_previousEnemy = enemyTransform;
        m_PetController.CloudExplosionClientRpc();
        m_PetController.TeleportCloseToEnemy(enemyTransform);
        m_PetController.CloudExplosionClientRpc();
        m_PetController.SpawnAttackAnimationClientRpc();
        m_PetController.ApplyDamageToEnemy(enemyTransform);
    }
}