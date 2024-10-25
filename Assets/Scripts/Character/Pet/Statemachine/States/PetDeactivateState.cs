using UnityEngine;

public class PetDeactivateState : PetState
{
    public PetDeactivateState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {
    }

    public override void Enter()
    {
        m_PetController.CloudExplosionClientRpc();
        m_PetController.DeactivateAgent();
        m_PetController.DeactivatePetViewClientRpc();
        m_PetController.SubscribeOnEnemyGetDamage();
    }

    public override void Exit()
    {
        m_PetController.UnsubscribeOnEnemyGetDamage();
    }

    public override void Update()
    {
        if (!m_PetController.IsEnemyInPlayerRange())
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetFollowOwnerState);
        }

        if (!m_PetController.IsPetMeterFullyCharged())
        {
            m_PetController.DrainPetMeter();
        }

        m_PetController.SetPetMeterProgress();

        if (Input.GetKeyDown(KeyCode.R) && m_PetController.IsPetMeterFullyCharged())
        {
            m_PetController.ResetPetMeter();
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetAttackState);
        }
    }
}
