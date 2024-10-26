public class PetFollowOwnerState : PetState
{
    public PetFollowOwnerState(PetController petController, PetStateMachine petStateMachine) : base(petController, petStateMachine)
    {

    }

    public override void Enter()
    {
        m_PetController.ActivateAgent();
        m_PetController.TeleportCloseToPlayer();
        m_PetController.CloudExplosionClientRpc();
        m_PetController.ActivatePetViewClientRpc();
    }

    public override void Exit()
    {
        m_PetController.RemoveDestination();
    }

    public override void Update()
    {
        m_PetController.FollowOwner();
        m_PetController.SetFacingDirection();

        if (m_PetController.IsEnemyInPlayerRange())
        {
            m_PetStateMachine.ChangeState(m_PetStateMachine.PetDeactivateState);
        }
    }
}
