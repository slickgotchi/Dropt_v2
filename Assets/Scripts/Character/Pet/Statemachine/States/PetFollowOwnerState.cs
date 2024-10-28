public class PetFollowOwnerState : PetState
{
    public PetFollowOwnerState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {

    }

    public override void Enter()
    {
        m_PetController.ActivateAgent();
        m_PetController.TeleportCloseToPlayer();
        m_PetController.SetFacingDirectionToOwnner();
        m_PetController.CloudExplosionClientRpc();
        m_PetController.ActivatePet();
    }

    public override void Exit()
    {
        m_PetController.RemoveDestination();
    }

    public override void Update()
    {
        m_PetController.FollowOwner();
        if (m_PetController.IsPlayerOutOfTeleportRange())
        {
            m_PetController.CloudExplosionClientRpc();
            m_PetController.TeleportCloseToPlayer();
            m_PetController.SetFacingDirectionToOwnner();
            m_PetController.CloudExplosionClientRpc();
        }
        m_PetController.SetFacingDirection();

        if (m_PetController.IsEnemyInPlayerRange())
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetDeactivateState);
        }

        m_PetController.DrainPetMeter();
        m_PetController.SetPetMeterProgress();
    }
}
