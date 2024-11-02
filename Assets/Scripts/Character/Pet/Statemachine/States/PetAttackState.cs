using UnityEngine;

public class PetAttackState : PetState
{
    private float m_attackInterval;
    private float m_counter;
    private Transform m_previousEnemy;

    public PetAttackState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {
    }

    public override void Enter()
    {
        m_attackInterval = m_PetController.GetAttackInterval();
        m_PetController.ResetSummonDuration();
        AttackToEnemy();
    }

    public override void Exit()
    {
        m_counter = 0;
    }

    public override void Update()
    {
        m_counter += Time.deltaTime;
        m_PetController.DrainSummonDuration();
        m_PetController.SetSummonProgress();
        if (m_PetController.IsSummonDurationOver())
        {
            m_PetStateMachine.ChangeState(m_PetController.IsEnemyInPlayerRange()
                                          ? m_PetStateMachine.PetDeactivateState
                                          : m_PetStateMachine.PetFollowOwnerState);
            return;
        }

        if (m_counter < m_attackInterval) return;

        m_counter = 0;
        AttackToEnemy();
    }

    private void AttackToEnemy()
    {
        Transform enemyTransform = m_PetController.GetEnemyInPlayerRange(m_previousEnemy);
        if (enemyTransform == null)
        {
            if (m_PetController.IsDeactivated())
            {
                return;
            }

            m_PetController.CloudExplosionClientRpc();
            m_PetController.DeactivatePet();
            return;
        }
        m_previousEnemy = enemyTransform;
        m_PetController.CloudExplosionClientRpc();
        m_PetController.TeleportCloseToEnemy(enemyTransform);
        m_PetController.ActivatePet();
        m_PetController.CloudExplosionClientRpc();
        m_PetController.SetFacingDirection(enemyTransform);
        m_PetController.SpawnAttackAnimation(enemyTransform);
        m_PetController.ApplyDamageToEnemy(enemyTransform);
    }
}